using ArgSharp;
using ArgSharp.Args;

namespace ArgSharpCmd
{
    internal static class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                ArgSharpClass.OnHelpInvoked = () => Environment.Exit(0);

                ArgSharpClass.Init("ArgSharpCmd", "ArgSharp Command Test", "Description 1", "Epilogue");
                ArgSharpClass.AddArgument<string>(["-path"], "Path", "A file path.");
                ArgSharpClass.AddArgument(["-sw", "--switch"], helpMsg: "A switch", defaultValue: false, isRequired: false);
                ArgSharpClass.AddExample("ArgSharpCmd -sw");
                ArgSharpClass.ArgumentZeroAction = ArgSharpClass.ArgZeroAction.ShowHelp;

                var profile = ArgSharpClass.AddArgumentAction(["profile", "p"], null, "Performs show profiles.");

                profile.AddArgument(["test"], helpMsg: "Show test", defaultValue: "test1");
                var prof2 = profile.AddArgumentAction(["run"], () =>
                {
                    var items = profile.GetArgStoreValues();
                    if (items.SingleOrDefault(a => a.Parameters.Contains("test")) is ArgStore<string> test)
                    {
                        Console.WriteLine($"Value {test.TypedValue}");
                    } else
                    {
                        Console.WriteLine("Test argument not found.");
                    }

                }, "Performs run test (show output).");
                prof2.ArgumentZeroAction = ArgSharpClass.ArgZeroAction.TreatAsSuccess;
                var res = ArgSharpClass.Parse(args);
                Console.WriteLine(res);
                if (!res) return;

                Console.WriteLine("Argument Stores:");
                foreach (var arg in ArgSharpClass.GetArgStoreValues())
                {
                    Console.WriteLine($"{string.Join(", ", arg.Parameters)} >> {arg.Value}");
                }
            }
            catch (ArgumentParseException apEx)
            {
                Console.WriteLine($"{apEx.Message} Type -h or --help for more info.");
            }
        }
    }
}
