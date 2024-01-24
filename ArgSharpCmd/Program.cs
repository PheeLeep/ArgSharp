using ArgSharp;

namespace ArgSharpCmd {
    internal static class Program {

        [STAThread]
        static void Main(string[] args) {
            try {
                ArgSharpClass.Init("ArgSharpCmd", "ArgSharp Command Test", "Description 1", "Epilogue");
                ArgSharpClass.AddArgument(ArgSharpClass.ArgumentAction.Store, new[] { "-path" }, "Path",
                                          "A file path.");
                ArgSharpClass.AddArgument(ArgSharpClass.ArgumentAction.Switch, new[] { "-sw", "--switch" },
                                          helpMsg: "A switch.");
                ArgSharpClass.AddExample("ArgSharpCmd -sw");
                ArgSharpClass.Parse(args);

                Console.WriteLine("Argument Stores:");
                foreach (var arg in ArgSharpClass.GetArgStoreValues()) {
                    Console.WriteLine($"{string.Join(", ", arg.Parameters)} >> {arg.Value}");
                }

                Console.WriteLine("\nArgument Switches:");
                foreach (var arg in ArgSharpClass.GetArgSwitchValues()) {
                    Console.WriteLine($"{string.Join(", ", arg.Parameters)} >> {arg.Value}");
                }
            } catch (ArgumentParseException apEx) {
                Console.WriteLine($"{apEx.Message} Type -h or --help for more info.");
            }
        }
    }
}
