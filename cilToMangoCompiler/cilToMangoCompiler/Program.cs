using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace cilToMango
{
    public class MainClass
    {
        private static System.IO.StreamWriter outputFile;
        private static Instruction currentInstruction;

        public static void Main(string[] args)
        {
            // startup things
            if (args.Length < 1)
            {
                Console.WriteLine("Please give the input file as first argument. Second for outputfile may be optional.");
                return;
            }
            Console.WriteLine("Input File: " + args[0]);
            string inputFileName = Path.GetFileName(args[0]);
            if (args.Length > 1)
            {
                outputFile = new System.IO.StreamWriter(args[1]);
            }
            else
            {
                outputFile = new System.IO.StreamWriter(inputFileName + ".mango");
            }
            ModuleDefinition module = ModuleDefinition.ReadModule(args[0]);


            //begin writing to outputfile
            PrintLn("module Main\n{");
            int externMethodCounter = 0;
            TypeDefinition mainClass = module.Types.Single(type => type.Name == "MainClass");
            foreach (MethodDefinition method in mainClass.Methods)
            {
                String methodName = method.Name;

                if (methodName.StartsWith(".")) // not relevant for the mango code
                {
                    Console.WriteLine("//Ignoring Instructions of Method " + methodName + "\n");
                    continue;
                }
                if (methodName.StartsWith("SYS") && methodName.Length > 3) // extern systemcall
                {
                    PrintLn("\tdeclare "
                            + GetMangoType(method.ReturnType.ToString())
                            + " @" + methodName + "("
                            + GetArgumentsFromMethod(method)
                            + ") " + (100 + externMethodCounter++) + "\n");
                    continue;
                }

                // normal function
                Print("\tdefine "
                        + GetMangoType(method.ReturnType.ToString())
                      + " @" + methodName + "(");
                Print(GetArgumentsFromMethod(method));
                PrintLn(")");
                PrintLn("\t{");

                ArrayList usedVariables = new ArrayList();
                ArrayList labelOffsets = new ArrayList();

                // search for branching and variables
                foreach (Instruction i in method.Body.Instructions)
                {
                    currentInstruction = i;
                    String opcodeName = i.OpCode.Name;

                    switch (opcodeName)
                    {
                        case "stloc.0":
                        case "stloc.1":
                        case "stloc.2":
                        case "stloc.3":
                            if (!usedVariables.Contains(opcodeName.Substring(6,1)))
                            {
                                usedVariables.Add(opcodeName.Substring(6, 1));
                            }
                            break;
                        case "br":
                        case "br.s":
                        case "brfalse":
                        case "brfalse.s":
                        case "brtrue":
                        case "brtrue.s":
                            if (!labelOffsets.Contains(((Instruction)i.Operand).Offset))
                            {
                                labelOffsets.Add(((Instruction)i.Operand).Offset);
                            }
                            break;
                    }
                }

                // prepare use of Variables
                foreach (String s in usedVariables)
                {
                    outputFile.WriteLine("\t\tlocal\ti32\t%loc" + s);
                }

                // translate instructions from cil to mango
                foreach (Instruction i in method.Body.Instructions)
                {
                    currentInstruction = i;
                    String opcodeName = i.OpCode.Name;

                    // check for Labels
                    if (labelOffsets.Contains(i.Offset)) {
                        Print("l" + labelOffsets.IndexOf(i.Offset) + ":\t\t");
                    }
                    else
                    {
                        Print("\t\t");
                    }

                    // fancy Debug output
                    Console.Write(i.Offset + "\t" + i.OpCode.Name);
                    if (i.Operand != null)
                    {
                        switch (i.Operand.GetType().Name)
                        {
                            case "Instruction":
                                Console.WriteLine("\t->\t" + ((Instruction)i.Operand).Offset);
                                break;
                            case "MethodDefinition":
                                Console.WriteLine("\t->\t" + ((MethodDefinition)i.Operand).Name);
                                break;
                            case "VariableDefinition":
                                Console.WriteLine("\t->\t" + ((VariableDefinition)i.Operand).ToString());
                                break;
                            default:
                                Console.WriteLine("\t\t" + i.Operand);
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("");
                    }


                    //compare the first 3 chars
                    int targetOffset;
                    switch (opcodeName)
                    {
                        case "add":
                            outputFile.WriteLine("add");
                            break;
                        case "br":
                        case "br.s":
                            targetOffset = ((Instruction)i.Operand).Offset;
                            outputFile.WriteLine("br\tl" + labelOffsets.IndexOf(targetOffset));
                            break;
                        case "brfalse":
                        case "brfalse.s":
                            targetOffset = ((Instruction)i.Operand).Offset;
                            outputFile.WriteLine("brfalse\tl" + labelOffsets.IndexOf(targetOffset));
                            break;
                        case "brtrue":
                        case "brtrue.s":
                            targetOffset = ((Instruction)i.Operand).Offset;
                            outputFile.WriteLine("brtrue\tl" + labelOffsets.IndexOf(targetOffset));
                            break;
                        case "call":
                            MethodDefinition targetMethod = ((MethodDefinition)i.Operand);
                            if (targetMethod.Name.StartsWith("SYS"))
                            {
                                outputFile.Write("sys");
                            }
                            outputFile.WriteLine("call\t"
                                             + GetMangoType(targetMethod.ReturnType.ToString())
                                             + "\t@" + targetMethod.Name + "("
                                             + GetArgumentsFromMethod(targetMethod, false)
                                             + ")"
                                            );
                            break;
                        case "ceq":
                            outputFile.WriteLine("ceq");
                            break;
                        case "cgt":
                            outputFile.WriteLine("cgt");
                            break;
                        case "clt":
                            outputFile.WriteLine("clt");
                            break;
                        case "div":
                            outputFile.WriteLine("div");
                            break;
                        case "ldarg.0":
                        case "ldarg.1":
                        case "ldarg.2":
                        case "ldarg.3":
                            outputFile.WriteLine("ldarg\t%arg" + opcodeName.Substring(6, 1));
                            break;
                        case "ldarg.s":
                            outputFile.WriteLine("ldarg\t%arg" + ((ParameterDefinition)i.Operand).Sequence);
                            break;
                        case "ldc.i4.m1":
                        case "ldc.i4.M1":
                            outputFile.WriteLine("ldc i32 -1");
                            break;
                        case "ldc.i4.0":
                        case "ldc.i4.1":
                        case "ldc.i4.2":
                        case "ldc.i4.3":
                        case "ldc.i4.4":
                        case "ldc.i4.5":
                        case "ldc.i4.6":
                        case "ldc.i4.7":
                        case "ldc.i4.8":
                            outputFile.WriteLine("ldc\ti32\t" + opcodeName.Substring(7, 1));
                            break;
                        case "ldc.i4.s":
                            outputFile.WriteLine("ldc\ti32\t" + i.Operand);
                            break;
                        case "ldloc.0":
                        case "ldloc.1":
                        case "ldloc.2":
                        case "ldloc.3":
                            outputFile.WriteLine("ldloc\t%loc" + opcodeName.Substring(6, 1));
                            break;
                        case "ldloc.s":
                            if (i.Operand != null && i.Operand.ToString().StartsWith("V_")
                                && i.Operand.ToString().Length >= 3)
                            {
                                outputFile.WriteLine("ldloc\t%loc" + i.Operand.ToString().Substring(2));
                                break;
                            }
                            OpcodeUnknown();
                            break;
                        case "mul":
                            outputFile.WriteLine("mul");
                            break;
                        case "nop":
                            outputFile.WriteLine("nop");
                            break;
                        case "ret":
                            outputFile.WriteLine("ret");
                            break;
                        case "stloc.0":
                        case "stloc.1":
                        case "stloc.2":
                        case "stloc.3":
                            outputFile.WriteLine("stloc\t%loc" + opcodeName.Substring(6,1));
                            break;
                        case "stloc.s":
                            if (i.Operand != null && i.Operand.ToString().StartsWith("V_")
                                && i.Operand.ToString().Length >= 3)
                            {
                                outputFile.WriteLine("stloc\t%loc" + i.Operand.ToString().Substring(2));
                                break;
                            }
                            OpcodeUnknown();
                            break;
                        case "sub":
                            outputFile.WriteLine("sub");
                            break;
                        default:
                            OpcodeUnknown();
                            break;
                    }
                }
                PrintLn("\t}\n\n");
            }
            outputFile.WriteLine("}");
            outputFile.Close();
        }

        // handles an unknokn opcode
        private static void OpcodeUnknown()
        {
            Console.WriteLine(">> Unknown Opcode:\t" + currentInstruction.OpCode.Name);
            Console.WriteLine(">> Operand:\t" + currentInstruction.Operand);
            outputFile.WriteLine("");
        }

        // converts a cil return type into a mango return type
        private static String GetMangoType(String cilType)
        {
            if (cilType == "")
            {
                return "";
            }
            switch (cilType)
            {
                case "System.Int32":
                    return "i32";
                default:
                    return "void";
            }
        }

        private static String GetArgumentsFromMethod(MethodDefinition m, bool withArgumentName = true)
        {
            // maybe throw runtime errors for invalid input
            // there could be a better way using regex
            String arguments = m.FullName.Substring(m.FullName.IndexOf("(") + 1);
            arguments = arguments.Substring(0, arguments.Length - 1);
            if (arguments == "")
            {
                return "";
            }
            String result = "";
            int argCounter = 0;
            foreach (String argument in arguments.Split(",".ToCharArray()))
            {
                if (result != "")
                {
                    result += ", ";
                }
                result += GetMangoType(argument);
                if (withArgumentName)
                {
                    result += " %arg" + argCounter++;
                }
            }
            return result;
        }

        private static void Print(String s) {
            Console.Write(s);
            outputFile.Write(s);
        }

        private static void PrintLn(String s)
        {
            Print(s + "\n");
        }
    }
}