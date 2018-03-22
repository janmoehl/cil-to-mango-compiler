using System;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace cilToMango
{
    public class MainClass
    {

        //private readonly string _targetFileName;
        //private readonly ModuleDefinition _module;

        public static void Main(string[] args)
        {
            string fileName = "../../../../SimpleExample/SimpleExample/bin/Debug/SimpleExample.exe";
            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);

            TypeDefinition mainClass = module.Types.Single(type => type.Name == "MainClass");
            foreach (MethodDefinition method in mainClass.Methods)
            {
                String methodName = method.Name;
                if (methodName.StartsWith("."))
                {
                    Console.WriteLine("Ignoring Instructions of Method " + methodName + "\n");
                }
                else
                {
                    Console.WriteLine("\nInstructions of Method " + methodName + "\n--------------------");
                    foreach (Instruction i in method.Body.Instructions)
                    {
                        switch (i.OpCode.Name) {
                            case "nop":
                                Console.WriteLine("[NOP]");
                                break;
                            case "ldc.i4.4":
                                Console.WriteLine("ldc i32 4");
                                break;
                            default:
                                Console.WriteLine("Unknown Opcode: " + i.OpCode.Name);
                                break;
                        }
                    }
                }
                Console.Write("\n\n");
            }
        }
    }
}
