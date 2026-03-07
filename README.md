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

It should be noted that invoking `ArgSharpClass.AddArgument()` has some specific usage if the parameter on the commandline argument matches during parsing:
- `AddArgumentAction(string[], Action, string)`: used to store an Action to be invoked.
- `AddArgument(string[], string, string, T)`:  used to store a variable or act as switch (if T is boolean).


Example:
```csharp
ArgSharpClass.AddArgument(new[] { "-path" }, "Path", "A file path.");
ArgSharpClass.AddArgument(new[] { "-sw", "--switch" }, helpMsg: "A switch. ABCDEFGHIJKLM\nOPQRSTUIVWXYZ", defaultValue: false);
```
After it has been added, you can now invoke `ArgSharpClass.Parse()` to parse the commandline argument. (If the `string[]`-value was not provided to `ArgSharpClass.Parse()`,
the parser will parse arguments from `Environment.GetCommandLineArgs()` instead.)

## Note

In 2.0, the `Parse()` will not throw an `ArgumentParseException` if parsing fails, instead
it will print the usage and error, before returning a false.
```csharp
   if (!ArgSharpClass.Parse(args)) return;
```

## Deprecation
These are the methods and classes that are deprecated following by 2.0 update, and it will be removed in the future update.
    - `AddArgument(string[], Action, string)`: renamed to `AddArgumentAction(string[], Action, string)`
    - `AddArgument(string[], bool, string)`: replaced by Type aware `AddArgument<T>`
    - `AddArgument(string[], string, string)`: replaced by Type aware `AddArgument<T>`
    - `GetArgSwitchValues()`: redundant. Use `GetArgStoreValues()` instead.
    - `public class ArgSwitch`: redundant.

## License
This source code is under [MIT License.](https://github.com/PheeLeep/ArgSharp/blob/master/LICENSE.txt)
