# README

## Summary
This project contains code for transforming a subset of the common intermediate
language (CIL) to mango assembler.

## Requirements
* Install mono (complete)

## Features
Until now, following basic C# commands are supported:
* variables of type int, bool (with workaround)
* basic arithmethic functions (+,-,*,/)
* structs
* if/else with boolean expressions
* switch/case/default/break
* ternary operator (?:)
* while
* public static void/int/bool methods
* pointer

But for some operations more testing is required.

## Build
Build the executable with
`xbuild cilToMangoCompiler/cilToMangoCompiler/cilToMangoCompiler.csproj`. Use
mono to execute the program. TODO: The executable build with xbuild contains at
the current time no reference to the used library mono.cecil. Use monodevelop at
this moment, to compile the executable.

Example:
If you want to create mango assembler from the blink example,this can
be done with the following steps:

1. Compile the C# program to CIL bytecode
`mcs blink/Program.cs`
2. Generate mango assembler from CIL bytecode
`mono cilToMangoCompiler/cilToMangoCompiler/bin/Debug/cilToMangoCompiler.exe blink/Program.exe blink/Program.mango`

The generated mango assembler code was saved in the file blink/Program.mango.
To see the original CIL Assembler you can use `monodis blink/Program.exe`
