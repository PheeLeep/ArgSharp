<div style="text-align:center">
    <img src="https://github.com/PheeLeep/ArgSharp/blob/master/Images/ArgSharpIcon.png"/>
</div>

# ArgSharp
A simple library that parses the program's commandline argument. The library is inspired by Python's [argparse](https://docs.python.org/3/library/argparse.html) 
module where it parses commandline arguments, stores values, and prints help and usage.

## Usage

- ArgSharp must initialize first before parsing an argument. Usually by invoking an `ArgSharpClass.Init()` (replace "ArgSharp" inside with your program name.)
```csharp
ArgSharpClass.Init("ArgSharpCmd");
```
- Or, you can add a title, description, and epilogue when initializing and it will show when passing `-h` or `--help` to the commandline argument.
```csharp
ArgSharpClass.Init("ArgSharpCmd", "ArgSharp Command Test", "Description 1", "Epilogue");
```

After it has been initialized, you are now ready to add an argument by invoking `ArgSharpClass.AddArgument()`.
### Note
It should be noted that invoking `ArgSharpClass.AddArgument()` has some specific usage if the parameter on the commandline argument matches during parsing:
- `AddArgument(string[], Action, string)`: used to store an Action to be invoked.
- `AddArgument(string[], bool, string)`: used as a switch.
- `AddArgument(string[], string, string)`: used to store a string variable.

Example:
```csharp
ArgSharpClass.AddArgument(new[] { "-path" }, "Path", "A file path.");
ArgSharpClass.AddArgument(new[] { "-sw", "--switch" }, false, "A switch. ABCDEFGHIJKLM\nOPQRSTUIVWXYZ");
```
After it has been added, you can now invoke `ArgSharpClass.Parse()` to parse the commandline argument. (If the `string[]`-value was not provided to `ArgSharpClass.Parse()`,
the parser will parse arguments from `Environment.GetCommandLineArgs()` instead.)

## Note
- The parsing operation must be inside the `try...catch` block as `ArgumentParseException` will be thrown if parsing fails.
- When the parsing fails, the parser will print the usage first before throwing an `ArgumentParseException` error.

## License
This source code is under [MIT License.](https://github.com/PheeLeep/ArgSharp/blob/master/LICENSE.txt)
