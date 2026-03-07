using ArgSharp;

namespace ArgSharpCmd
{
    internal static class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                ArgSharpClass.Init("ArgSharpCmd", "ArgSharp Command Test", "Description 1", "Epilogue");
                ArgSharpClass.AddArgument<string>(["-path"], "Path", "A file path.");
                ArgSharpClass.AddArgument(["-sw", "--switch"], helpMsg: "A switch", defaultValue: false);
                ArgSharpClass.AddExample("ArgSharpCmd -sw");
                if (!ArgSharpClass.Parse(args)) return;

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
