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
        private static Instruction currentInstruction;

        public static void Main(string[] args)
        {
            // startup things
            if (args.Length < 1 || args.Length > 2)
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

            // handle c# structs
            foreach (TypeDefinition type in mainClass.NestedTypes)
            {
                PrintLn("\ttype " + type.Name);
                PrintLn("\t{");
                foreach (FieldDefinition field in type.Fields)
                {
                    PrintLn("\t\tfield\t " + GetMangoType(field.FieldType) + " " + field.Name);
                }
                PrintLn("\t}\n\n");
            }
            foreach (MethodDefinition method in mainClass.Methods)
            {
                String methodName = method.Name.ToLower();

                if (methodName.StartsWith(".")) // not relevant for the mango code
                {
                    Console.WriteLine("//Ignoring Instructions of Method " + methodName + "\n");
                    continue;
                }
                if (methodName.StartsWith("sys") && methodName.Length > 3) // extern systemcall
                {
                    PrintLn("\tdeclare "
                            + GetMangoType(method.ReturnType)
                            + " @" + methodName + "("
                            + GetArgumentsFromMethod(method)
                            + ") " + (100 + externMethodCounter++) + "\n");
                    continue;
                }

                // normal function
                Print("\tdefine "
                      + GetMangoType(method.ReturnType)
                      + " @" + methodName + "(");
                Print(GetArgumentsFromMethod(method));
                PrintLn(")");
                PrintLn("\t{");

                ArrayList labelOffsets = new ArrayList();

                // search for branching instructions
                foreach (Instruction i in method.Body.Instructions)
                {
                    currentInstruction = i;
                    String opcodeName = i.OpCode.Name;

                    switch (opcodeName)
                    {
                        case "beq":
                        case "beq.s":
                        case "bge":
                        case "bge.s":
                        case "bge.un":
                        case "bge.un.s":
                        case "bgt":
                        case "bgt.s":
                        case "bgt.un":
                        case "bgt.un.s":
                        case "ble":
                        case "ble.s":
                        case "ble.un":
                        case "ble.un.s":
                        case "blt":
                        case "blt.s":
                        case "blt.un":
                        case "blt.un.s":
                        case "bne.un":
                        case "bne.un.s":
                        case "br":
                        case "br.s":
                        case "brfalse":
                        case "brfalse.s":
                        case "brnull":
                        case "brnull.s":
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
                int localVarIndex = 0;
                foreach (VariableDefinition var in method.Body.Variables)
                {
                    string varType;
                    if (var.VariableType.IsPrimitive)
                    {
                        varType = GetMangoType(var.VariableType);
                    }
                    else
                    {
                        varType = var.VariableType.Name;
                    }
                    outputFile.WriteLine("\t\tlocal\t" + varType + "\t%loc" + localVarIndex++);
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


                    int targetOffset;   // used for branching
                    FieldReference targetField; // used for stfld
                    string fieldName;    // used for stfld
                    switch (opcodeName)
                    {
                        case "add":
                            outputFile.WriteLine("add");
                            break;
                        case "beq":
                        case "beq.s":
                        case "bge":
                        case "bge.s":
                        case "bge.un":
                        case "bge.un.s":
                        case "bgt":
                        case "bgt.s":
                        case "bgt.un":
                        case "bgt.un.s":
                        case "ble":
                        case "ble.s":
                        case "ble.un":
                        case "ble.un.s":
                        case "blt":
                        case "blt.s":
                        case "blt.un":
                        case "blt.un.s":
                        case "bne.un":
                        case "bne.un.s":
                        case "br":
                        case "br.s":
                            String mangoInstruction;
                            if (opcodeName.EndsWith(".s"))
                            {
                                mangoInstruction = opcodeName.Substring(0, opcodeName.Length - 2);
                            }
                            else
                            {
                                mangoInstruction = opcodeName;
                            }
                            targetOffset = ((Instruction)i.Operand).Offset;
                            outputFile.WriteLine(mangoInstruction + "\tl" + labelOffsets.IndexOf(targetOffset));
                            break;
                        case "brfalse":
                        case "brfalse.s":
                        case "brnull":
                        case "brnull.s":
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
                            if (targetMethod.Name.ToLower().StartsWith("sys"))
                            {
                                outputFile.Write("sys");
                            }
                            outputFile.WriteLine("call\t"
                                                 + GetMangoType(targetMethod.ReturnType)
                                             + "\t@" + targetMethod.Name.ToLower() + "("
                                             + GetArgumentsFromMethod(targetMethod, false)
                                             + ")"
                                            );
                            break;
                        case "ceq":
                        case "cgt":
                        case "cgt.un":
                        case "clt":
                        case "clt.un":
                        case "dup":
                            outputFile.WriteLine(opcodeName);
                            break;
                        case "idind.u8": // not ldind.u8 !
                            outputFile.WriteLine("ldind i64");
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
                        case "ldc.i4":
                        case "ldc.i4.s":
                            outputFile.WriteLine("ldc\ti32\t" + i.Operand);
                            break;
                        case "ldfld":
                        case "ldflda":
                            if (i.Operand is FieldReference)
                            {
                                targetField = (FieldReference)i.Operand;
                                outputFile.Write(opcodeName + "\t");
                                if (targetField.FieldType.IsPrimitive)
                                {
                                    outputFile.Write(GetMangoType(targetField.FieldType) + "\t");
                                }
                                else
                                {
                                    outputFile.Write(targetField.FieldType.Name + "\t");
                                }
                                fieldName = targetField.FullName; // e.g. "System.Int32 SimpleExample.MainClass/Point2D::x"
                                fieldName = fieldName.Substring(fieldName.IndexOf('/') + 1); // e.g. "Point2D::x"
                                fieldName = fieldName.Replace("::", "/");   // e.g. "Point2D/x"
                                outputFile.WriteLine(fieldName);
                            }
                            break;
                        // case "ldind.i":
                        case "ldind.i1":
                            outputFile.WriteLine("ldind i8");
                            outputFile.WriteLine("\t\tconv i32");
                            break;
                        case "ldind.i2":
                            outputFile.WriteLine("ldind i16");
                            outputFile.WriteLine("\t\tconv i32");
                            break;
                        case "ldind.i4":
                            outputFile.WriteLine("ldind i32");
                            break;
                        case "ldind.i8":
                            outputFile.WriteLine("ldind i64");
                            break;
                        case "ldind.r4":
                            outputFile.WriteLine("ldind f32");
                            break;
                        case "ldind.r8":
                            outputFile.WriteLine("ldind f64");
                            break;
                        //case "ldind.ref":
                        case "ldind.u1":
                            outputFile.WriteLine("ldind u8");
                            outputFile.WriteLine("\t\tconv i32");
                            break;
                        case "ldind.u2":
                            outputFile.WriteLine("ldind u16");
                            outputFile.WriteLine("\t\tconv i32");
                            break;
                        case "ldind.u4":
                            outputFile.WriteLine("ldind u32");
                            outputFile.WriteLine("\t\tconv i32");
                            break;
                        case "ldloca":
                        case "ldloca.s":
                            if (i.Operand != null && i.Operand.ToString().StartsWith("V_")
                                && i.Operand.ToString().Length >= 3)
                            {
                                outputFile.WriteLine("ldloca\t%loc" + i.Operand.ToString().Substring(2));
                            }
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
                            }
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
                        case "stfld":
                            if (i.Operand is FieldReference)
                            {
                                targetField = (FieldReference) i.Operand;
                                outputFile.Write("stfld\t");
                                if (targetField.FieldType.IsPrimitive)
                                {
                                    outputFile.Write(GetMangoType(targetField.FieldType) + "\t");
                                }
                                else
                                {
                                    outputFile.Write(targetField.FieldType.Name + "\t");
                                }
                                fieldName = targetField.FullName; // e.g. "System.Int32 SimpleExample.MainClass/Point2D::x"
                                fieldName = fieldName.Substring(fieldName.IndexOf('/') + 1); // e.g. "Point2D::x"
                                fieldName = fieldName.Replace("::", "/");   // e.g. "Point2D/x"
                                outputFile.WriteLine(fieldName);
                            }
                            break;
                        //case "stind.ref":
                        case "stind.i1":
                            outputFile.WriteLine("stind i8");
                            break;
                        case "stind.i2":
                            outputFile.WriteLine("stind i16");
                            break;
                        case "stind.i4":
                            outputFile.WriteLine("stind i32");
                            break;
                        case "stind.i8":
                            outputFile.WriteLine("stind i64");
                            break;
                        case "stind.r4":
                            outputFile.WriteLine("stind f32");
                            break;
                        case "stind.r8":
                            outputFile.WriteLine("stind f64");
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
                            break;
                        case "sub":
                            outputFile.WriteLine("sub");
                            break;
                        default:
                            Console.WriteLine(">> Unknown Opcode:\t" + currentInstruction.OpCode.Name);
                            Console.WriteLine(">> Operand:\t" + currentInstruction.Operand);
                            outputFile.WriteLine("");
                            break;
                    }
                }
                PrintLn("\t}\n\n");
            }
            outputFile.WriteLine("}");
            outputFile.Close();
        }

        // converts a cil return type into a mango return type
        private static String GetMangoType(TypeReference cilType)
        {
            if (!cilType.IsPrimitive)
            {
                if (cilType.Name == "Void")
                {
                    return "void";
                }
                return cilType.Name.ToString();
            }
            switch (cilType.Name)
            {
                case "Int32":
                    return "i32";
                default:
                    Console.WriteLine("Unknown Type:\t" + cilType.FullName);
                    return "void";
            }
        }

        // should return something like "int32, f32, int32" or "int32 %arg0, f32 %arg1"
        private static String GetArgumentsFromMethod(MethodDefinition m, bool withArgumentName = true)
        {
            string result = "";
            bool firstParameter = true;
            int argumentCounter = 0;
            foreach (ParameterDefinition p in m.Parameters)
            {
                if (!firstParameter)
                {
                    result += ", ";
                }
                else
                {
                    firstParameter = false;
                }
                result += GetMangoType(p.ParameterType);
                if (withArgumentName) {
                    result += " %arg" + argumentCounter++;
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