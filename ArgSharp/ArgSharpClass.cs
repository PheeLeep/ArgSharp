using ArgSharp.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArgSharp {

    /// <summary>
    /// A static class use for commandline argument construction, parsing, and getting values from
    /// specified parameters.
    /// </summary>
    public static class ArgSharpClass {
        private static bool isInitialized = false;
        private static readonly List<RootArgument> args = new List<RootArgument>();
        private static readonly List<string> examples = new List<string>();

        private static string progName;
        private static string description;
        private static string epilog;
        private static string title;

        /// <summary>
        /// An enumeration of specifying an argument behavior when the argument matched to the specified parameter.
        /// </summary>
        public enum ArgumentAction {
            /// <summary>
            /// The argument will store the value after its parameter. (for <see cref="ArgStore"/>)
            /// </summary>
            Store,

            /// <summary>
            /// The argument will switch <see cref="ArgSwitch"/> value to true when the argument matches against
            /// the specified parameter.
            /// </summary>
            Switch,

            /// <summary>
            /// The argument will invoke the <see cref="Action"/> specified in the <see cref="ArgInvoke"/> when
            /// the argument matched against the specified parameter.
            /// </summary>
            Invoke
        }

        /// <summary>
        /// Initializes the parser.
        /// </summary>
        /// <param name="progName">The name of the program. (example: ArgSharpCmd.exe or ArgSharpCmd)</param>
        /// <param name="title">The title of the program.</param>
        /// <param name="description">The description of the program.</param>
        /// <param name="epilog">The epilogue of the program after printing the help.</param>
        public static void Init(string progName, string title = "", string description = "", string epilog = "") {
            if (isInitialized) {
                Console.WriteLine("Argument parser already initialized.");
                return;
            }

            isInitialized = true;
            ArgSharpClass.progName = progName;
            ArgSharpClass.description = description;
            ArgSharpClass.epilog = epilog;
            ArgSharpClass.title = title;

            // First add the help.
            AddArgument(ArgumentAction.Invoke, new string[] { "-h", "--help" }, helpMsg: "Show this message and exit.",
                        a: new Action(() => InvokeHelp()));
        }

        /// <summary>
        /// Adds the argument and other parameters.
        /// </summary>
        /// <param name="action">The action when the parser detects the specified parameter.</param>
        /// <param name="parameters">The list of parameters for the specific argument.</param>
        /// <param name="valName">The value name. (must use <see cref="ArgumentAction.Store"/> in 
        /// <paramref name="action"/>)</param>
        /// <param name="helpMsg">The argument message that will display when invoking '-h' parameter.</param>
        /// <param name="a">
        /// The <see cref="Action"/> method to be invoked once the parameter matched. 
        /// (must use with <see cref="ArgumentAction.Invoke"/> in <paramref name="action"/>)
        /// </param>
        /// <exception cref="ArgumentParseException"></exception>
        public static void AddArgument(ArgumentAction action, string[] parameters, string valName = "(paramName)",
                                       string helpMsg = "", Action a = null) {
            RootArgument rA = null;
            switch (action) {
                case ArgumentAction.Store:
                    rA = new ArgStore {
                        ValueName = valName
                    };
                    break;
                case ArgumentAction.Switch:
                    rA = new ArgSwitch();
                    break;
                case ArgumentAction.Invoke:
                    rA = a == null ? null : new ArgInvoke(a);
                    break;
            }

            if (rA == null || !parameters.Any() || parameters.Any(r => string.IsNullOrWhiteSpace(r)))
                throw new ArgumentParseException("Argument failed to parse.");
            rA.Parameters = parameters;
            rA.HelpMessage = helpMsg;
            AddArg(rA);
        }

        /// <summary>
        /// Inserts the example of using your program.
        /// </summary>
        /// <param name="example">The example string.</param>
        public static void AddExample(string example) {
            if (string.IsNullOrWhiteSpace(example)) return;
            examples.Add(example);
        }

        /// <summary>
        /// Parses the arguments from <see cref="Environment.GetCommandLineArgs()"/>.
        /// </summary>
        public static void Parse() {
            List<string> argSpecified = new List<string>(Environment.GetCommandLineArgs());
            argSpecified.RemoveAt(0);
            Parse(argSpecified.ToArray());
        }

        /// <summary>
        /// Parses the arguments specified on the parameter.
        /// </summary>
        /// <param name="args">An array of string arguments.</param>
        /// <exception cref="ArgumentParseException"></exception>
        public static void Parse(string[] args) {
            if (args.Length == 0 || ArgSharpClass.args.Count == 0) return;

            for (int i = 0; i < args.Length; i++) {
                RootArgument arg = ArgSharpClass.args.FirstOrDefault(r => r.Parameters.Contains(args[i]));
                switch (arg) {
                    case null:
                        ShowUsage();
                        throw new ArgumentParseException($"Parameter '{args[i]}' is not found.");
                    case ArgStore av:
                        if ((i + 1) >= args.Length) {
                            ShowUsage();
                            throw new ArgumentParseException($"Value for '{args[i]}' is not found.");
                        }
                        i++;
                        av.Value = args[i];
                        continue;
                    case ArgSwitch asw:
                        asw.Value = true;
                        continue;
                    case ArgInvoke aInv:
                        aInv.Invoke();
                        continue;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ArgStore"/> variables and it's values.
        /// </summary>
        /// <returns>Returns the array of <see cref="ArgStore"/> classes.</returns>
        public static ArgStore[] GetArgStoreValues() {
            List<ArgStore> argStores = new List<ArgStore>();
            foreach (RootArgument ar in args)
                if (ar is ArgStore store)
                    argStores.Add(store);

            return argStores.ToArray();
        }

        /// <summary>
        /// Gets the <see cref="ArgSwitch"/> variables and it's values.
        /// </summary>
        /// <returns>Returns the array of <see cref="ArgSwitch"/> classes.</returns>
        public static ArgSwitch[] GetArgSwitchValues() {
            List<ArgSwitch> argStores = new List<ArgSwitch>();
            foreach (RootArgument ar in args)
                if (ar is ArgSwitch store)
                    argStores.Add(store);

            return argStores.ToArray();
        }

        /// <summary>
        /// Invokes the help and exits..
        /// </summary>
        internal static void InvokeHelp() {
            ShowUsage();
            if (!string.IsNullOrWhiteSpace(description))
                Console.WriteLine(description + "\n");
            ShowHelp();
            if (!string.IsNullOrWhiteSpace(epilog))
                Console.WriteLine(epilog);
            if (examples.Any()) {
                Console.WriteLine("\nExample/s:");
                foreach (string argExample in examples) {
                    Console.WriteLine($"\t{argExample}");
                }
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Adds an initialized <see cref="RootArgument"/>-derived class.
        /// </summary>
        /// <param name="argNew">The initialized <see cref="RootArgument"/>-derived class.</param>
        /// <exception cref="ArgumentParseException"></exception>
        private static void AddArg(RootArgument argNew) {
            foreach (var param in argNew.Parameters) {
                RootArgument arg = args.FirstOrDefault(r => r.Parameters.Contains(param));
                if (arg != null)
                    throw new ArgumentParseException($"Parameter '{param}' already added.");
            }
            args.Add(argNew);
        }

        /// <summary>
        /// Shows the help usage.
        /// </summary>
        private static void ShowUsage() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Count; i++) {
                RootArgument arg = args[i];
                sb.Append('[');
                for (int j = 0; j < arg.Parameters.Length; j++) {
                    sb.Append(arg.Parameters[j]);

                    if (arg is ArgStore argStore) {
                        sb.Append($" <{argStore.ValueName}>");
                    }
                    if ((j + 1) < arg.Parameters.Length)
                        sb.Append(" | ");
                }
                sb.Append(']');
                if ((i + 1) < args.Count)
                    sb.Append(' ');
            }

            if (!string.IsNullOrWhiteSpace(title))
                Console.WriteLine(title + "\n");
            Console.WriteLine("Usage:");
            string[][] arrUsage = {
                new []{progName , sb.ToString() }
            };
            Console.WriteLine(GenerateTable(arrUsage, 1));
        }

        /// <summary>
        /// Shows the help description.
        /// </summary>
        private static void ShowHelp() {
            List<string[]> helpArgs = new List<string[]>();
            foreach (RootArgument arg in args) {
                if (arg.Parameters != null) {
                    string[] argHelp = new string[2];
                    StringBuilder sb = new StringBuilder();

                    sb.Append('\t');
                    for (int i = 0; i < arg.Parameters.Length; i++) {
                        sb.Append(arg.Parameters[i]);

                        if (arg is ArgStore argStore) {
                            sb.Append($" <{argStore.ValueName}>");
                        }
                        if ((i + 1) < arg.Parameters.Length)
                            sb.Append(", ");
                    }
                    argHelp[0] = sb.ToString();
                    argHelp[1] = arg.HelpMessage;
                    helpArgs.Add(argHelp);
                }
            }
            Console.WriteLine("Options:");
            Console.WriteLine(GenerateTable(helpArgs.ToArray()));
        }

        /// <summary>
        /// Generates the table format containing 2D array strings.
        /// </summary>
        /// <param name="array">A two-dimensional array of strings.</param>
        /// <param name="padLength">The padded length of each rows, except in the last columns.</param>
        /// <returns>Returns the string containing table formatted values.</returns>
        private static string GenerateTable(string[][] array, int padLength = 3) {
            StringBuilder sb = new StringBuilder();
            int rows = array.Length;
            int cols = array[0].Length;

            // Calculate column widths
            int[] columnWidths = new int[cols - 1]; // Exclude the last column

            for (int j = 0; j < cols - 1; j++) {
                for (int i = 0; i < rows; i++) {
                    int length = array[i][j].Length;
                    columnWidths[j] = Math.Max(columnWidths[j], length);
                }
            }

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols - 1; j++)
                    sb.Append($"{array[i][j].PadRight(columnWidths[j] + padLength)}");
                sb.AppendLine($"{array[i][cols - 1]}");
            }
            return sb.ToString();
        }
    }
}
