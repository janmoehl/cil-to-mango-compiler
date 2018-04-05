using System;
using System.Collections;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace cilToMango
{
    public class MainClass
    {
        private static System.IO.StreamWriter outputFile;

        public static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
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


            outputFile.WriteLine("module Main");
            outputFile.WriteLine("{");

            TypeDefinition mainClass = module.Types.Single(type => type.Name == "MainClass");
            foreach (MethodDefinition method in mainClass.Methods)
            {
                String methodName = method.Name;
                if (methodName.StartsWith("."))
                {
                    Console.WriteLine("//Ignoring Instructions of Method " + methodName + "\n");
                }
                else
                {
                    outputFile.WriteLine("\tdefine void @" + methodName + "()");
                    outputFile.WriteLine("\t{");

                    ArrayList usedVariables = new ArrayList();
                    ArrayList labelOffsets = new ArrayList();

                    // search for branching and variables
                    foreach (Instruction i in method.Body.Instructions)
                    {
                        String opcodeName = i.OpCode.Name;

                        switch (opcodeName.Substring(0, Math.Min(4, opcodeName.Length)))
                        {
                            case "stlo":
                                if (opcodeName.StartsWith("stloc.") && opcodeName.Length == 7
                                    && !usedVariables.Contains(opcodeName.Substring(6,1)))
                                {
                                    usedVariables.Add(opcodeName.Substring(6, 1));
                                }
                                break;
                            case "brfa": //brfalse
                                if (opcodeName == "brfalse.s")
                                {
                                    labelOffsets.Add(((Instruction) i.Operand).Offset);
                                }
                                break;
                            case "br.s": //brfalse
                                if (opcodeName == "br.s")
                                {
                                    labelOffsets.Add(((Instruction)i.Operand).Offset);
                                }
                                break;
                        }
                    }

                    // prepare use of Variables
                    foreach (String s in usedVariables)
                    {
                        outputFile.WriteLine("\t\tlocal i32 %loc" + s);
                    }

                    // translate instructions from cil to mango
                    foreach (Instruction i in method.Body.Instructions)
                    {
                        String opcodeName = i.OpCode.Name;

                        // check for Labels
                        if (labelOffsets.Contains(i.Offset)) {
                            outputFile.Write("l" + labelOffsets.IndexOf(i.Offset) + ":\t\t");
                            Console.Write("l" + labelOffsets.IndexOf(i.Offset) + ":\t\t");
                        }
                        else
                        {
                            outputFile.Write("\t\t");
                            Console.Write("\t\t");
                        }

                        // fancy Debug
                        Console.WriteLine(i.Offset + "\t" + i.OpCode.Name);
                        if (i.Operand != null)
                        {
                            Console.WriteLine("\t\t -> " + ((Instruction)i.Operand).Offset);
                        }


                        //compare the first 3 chars
                        switch (opcodeName.Substring(0, Math.Min(4, opcodeName.Length)))
                        {
                            case "add":
                                outputFile.WriteLine("add");
                                break;
                            case "brfa": // brfalse
                                if (opcodeName == "brfalse.s")
                                {
                                    int targetOffset = ((Instruction) i.Operand).Offset;
                                    outputFile.WriteLine("brfalse\tl" + labelOffsets.IndexOf(targetOffset));
                                }
                               break;
                            case "br.s": // br.s
                                if (opcodeName == "br.s")
                                {
                                    int targetOffset = ((Instruction)i.Operand).Offset;
                                    outputFile.WriteLine("br\tl" + labelOffsets.IndexOf(targetOffset));
                                }
                                break;
                            case "clt":
                                outputFile.WriteLine("add");
                                break;
                            case "ldc.": // ldc
                                if (opcodeName.StartsWith("ldc.i4.") && opcodeName.Length == 8)
                                {
                                    outputFile.WriteLine("ldc\ti32\t" + opcodeName.Substring(7,1));
                                    break;
                                }
                                Console.WriteLine("Unknown Opcode: " + opcodeName);
                                break;
                            case "ldlo": //ldloc
                                if (opcodeName.StartsWith("ldloc.") && opcodeName.Length == 7)
                                {
                                    outputFile.WriteLine("ldloc\t%loc" + opcodeName.Substring(6, 1));
                                    break;
                                }
                                Console.WriteLine("Unknown Opcode:\t" + opcodeName);
                                break;
                            case "nop":
                                outputFile.WriteLine("nop");
                                break;
                            case "stlo":
                                if (opcodeName.StartsWith("stloc.") && opcodeName.Length == 7)
                                {
                                    outputFile.WriteLine("stloc %loc" + opcodeName.Substring(6, 1));
                                    break;
                                }
                                Console.WriteLine("Unknown Opcode:\t" + opcodeName);
                                break;
                            case "ret":
                                outputFile.WriteLine("ret");
                                break;
                            default:
                                Console.WriteLine("Unknown Opcode:\t" + opcodeName);
                                outputFile.WriteLine("");
                                break;
                        }
                    }
                    outputFile.WriteLine("\t}");
                }
            }
            outputFile.WriteLine("}");


            outputFile.Close();
        }
    }
}
