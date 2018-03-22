using System;
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
            Console.WriteLine("Opened " + fileName + " for reading");
            Console.WriteLine("File has " + module.Types.Count + "Types:");
            foreach (TypeDefinition type in module.Types)
            {
                Console.WriteLine(type.Name);
            }
        }
    }
}
