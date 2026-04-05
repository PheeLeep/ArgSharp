<div style="text-align:center">
    <img src="https://github.com/PheeLeep/ArgSharp/blob/master/Images/ArgSharpIcon.png"/>
</div>

# ArgSharp

![NuGet Version](https://img.shields.io/nuget/v/PheeLeep.ArgSharp)

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
- `AddArgumentAction(string[], Action?, string)`: used to store an Action to be invoked (see Command Chaining for details).
- `AddArgument(string[], string, string, T)`:  used to store a variable or act as switch (if T is boolean).

### Supported types
- int
- float
- long
- double
- decimal
- bool
- string

## Example:

### Simple

```csharp
ArgSharpClass.AddArgument(new[] { "-path" }, "Path", "A file path.");
ArgSharpClass.AddArgument(new[] { "-sw", "--switch" }, helpMsg: "A switch. ABCDEFGHIJKLM\nOPQRSTUIVWXYZ", defaultValue: false);
```
After it has been added, you can now invoke `ArgSharpClass.Parse()` to parse the commandline argument. (If the `string[]`-value was not provided to `ArgSharpClass.Parse()`,
the parser will parse arguments from `Environment.GetCommandLineArgs()` instead.)

### Command Chaining (on Version 2.3)

Command Chaining performs by creating `ArgInvoke` class from `AddArgumentAction` (either from the root command `ArgSharpClass` or other `ArgInvoke` objects)

During parsing, ArgSharp will check switches and parameter arguments first on its own before invoking the parameters for `ArgInvoke`. Other `ArgInvoke` objects that are attached will perform the same procedure.

```csharp
var profile = ArgSharpClass.AddArgumentAction(["profile", "p"], null, "Performs show profiles.");

profile.AddArgument(["test"], helpMsg: "Show test", defaultValue: "");
var prof2 = profile.AddArgumentAction(["expedite"], () =>
{
    // Perform some command here.
}, "Performs expedition.");
```

## Note

In 2.0, the `Parse()` will not throw an `ArgumentParseException` if parsing fails, instead
it will print the usage and error, before returning a false.
```csharp
   if (!ArgSharpClass.Parse(args)) return;
```

## License
This source code is under [MIT License.](https://github.com/PheeLeep/ArgSharp/blob/master/LICENSE.txt)
