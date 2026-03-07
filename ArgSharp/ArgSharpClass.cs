using ArgSharp.Args;
using PheeLeep.ArgSharp.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArgSharp
{

    /// <summary>
    /// A static class use for commandline argument construction, parsing, and getting values from
    /// specified parameters.
    /// </summary>
    public static class ArgSharpClass
    {
        private static bool isInitialized = false;
        private static readonly List<RootArgument> args = new List<RootArgument>();
        private static readonly List<string> examples = new List<string>();

        private static string progName;
        private static string description;
        private static string epilog;
        private static string title;

        /// <summary>
        /// An enumeration on how parser will behave if the argument length is zero.
        /// </summary>
        public enum ArgZeroAction
        {
            /// <summary>
            /// Exit the parser.
            /// </summary>
            Return,

            /// <summary>
            /// Shows the help output and exit the parser.
            /// </summary>
            ShowHelp,

            /// <summary>
            /// The parser will throw <see cref="ArgumentParseException"/> if the argument is zero.
            /// </summary>
            ShowError
        }

        /// <summary>
        /// An enumeration on how parser will behave if a user inputs a duplicate argument.
        /// </summary>
        public enum ArgStoredOrInvokedAction
        {
            /// <summary>
            /// Ignore the duplicate argument. 
            /// </summary>
            Ignore,

            /// <summary>
            /// Overwrite or re-invoke the argument.
            /// </summary>
            /// <remarks>
            /// This can cause an unintended consequences.
            /// </remarks>
            OverwriteOrReinvoke,

            /// <summary>
            /// Output an error when a duplicate argument occurs.
            /// </summary>
            ShowError
        }

        /// <summary>
        /// Gets or sets the parser's behavior if the commandline argument is zero.
        /// </summary>
        public static ArgZeroAction ArgumentZeroAction { get; set; } = ArgZeroAction.ShowHelp;

        /// <summary>
        /// Gets or sets the parser's behavior if the commandline has duplicate arguments.
        /// </summary>
        public static ArgStoredOrInvokedAction ArgumentStoredOrInvokedAction { get; set; } = ArgStoredOrInvokedAction.Ignore;

        /// <summary>
        /// Initializes the parser.
        /// </summary>
        /// <param name="progName">The name of the program. (example: ArgSharpCmd.exe or ArgSharpCmd)</param>
        /// <param name="title">The title of the program.</param>
        /// <param name="description">The description of the program.</param>
        /// <param name="epilog">The epilogue of the program after printing the help.</param>
        public static void Init(string progName, string title = "", string description = "", string epilog = "")
        {
            if (isInitialized)
            {
                Console.WriteLine("Argument parser already initialized.");
                return;
            }

            isInitialized = true;
            ArgSharpClass.progName = progName;
            ArgSharpClass.description = description;
            ArgSharpClass.epilog = epilog;
            ArgSharpClass.title = title;

            // First add the help.
            AddArgumentAction(new string[] { "-h", "--help" },
                              new Action(() => InvokeHelp()),
                              "Show this message and exit.");
        }

        /// <summary>
        /// Sets the argument to invoke a method if the parameter is matched.
        /// </summary>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="a">The provided <see cref="Action"/> method to be invoke later.</param>
        /// <param name="helpMsg">The help message.</param>
        /// <exception cref="ArgumentParseException"></exception>
        public static void AddArgumentAction(string[] parameters, Action a, string helpMsg = "")
        {
            if (a == null)
                throw new ArgumentParseException("Invoke not been given for the argument.");
            ArgInvoke argInvoke = new ArgInvoke(a)
            {
                Parameters = parameters,
                HelpMessage = helpMsg
            };
            InsertArgument(argInvoke);
        }

        /// <summary>
        /// Sets the argument to store a specific variable.
        /// </summary>
        /// <typeparam name="T">The specific variable type.</typeparam>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="placeHolder">A placeholder of the specific value name.</param>
        /// <param name="helpMsg">The help message.</param>
        /// <param name="defaultValue">The default value for the specified type.</param>
        public static void AddArgument<T>(string[] parameters,
                                          string placeHolder = "(value)",
                                          string helpMsg = "",
                                          T defaultValue = default)
        {
            var argStore = new ArgStore<T>(defaultValue)
            {
                Parameters = parameters,
                HelpMessage = helpMsg,
                ValueName = placeHolder
            };
            InsertArgument(argStore);
        }

        /// <summary>
        /// Inserts the example of using your program.
        /// </summary>
        /// <param name="example">The example string.</param>
        public static void AddExample(string example)
        {
            if (string.IsNullOrWhiteSpace(example)) return;
            examples.Add(example);
        }

        /// <summary>
        /// Parses the arguments from <see cref="Environment.GetCommandLineArgs()"/>.
        /// </summary>
        public static bool Parse()
        {
            List<string> argSpecified = new List<string>(Environment.GetCommandLineArgs());
            argSpecified.RemoveAt(0);
            return Parse(argSpecified.ToArray());
        }

        /// <summary>
        /// Parses the arguments specified on the parameter.
        /// </summary>
        /// <param name="args">An array of string arguments.</param>
        public static bool Parse(string[] args)
        {
            if (ArgSharpClass.args.Count == 0) return false;

            if (args.Length == 0)
            {
                switch (ArgumentZeroAction)
                {
                    case ArgZeroAction.Return:
                        break;
                    case ArgZeroAction.ShowHelp:
                        InvokeHelp();
                        break;
                    case ArgZeroAction.ShowError:
                        Console.WriteLine("No argument has been given.");
                        break;
                }

                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                RootArgument arg = ArgSharpClass.args.FirstOrDefault(r => r.Parameters.Contains(args[i]));
                if (arg == null)
                {
                    ShowUsage();
                    Console.WriteLine($"Parameter '{args[i]}' is not found.");
                    return false;
                }

                if (arg.IsArgStoredOrInvoked)
                {
                    switch (ArgumentStoredOrInvokedAction)
                    {
                        case ArgStoredOrInvokedAction.Ignore:
                            continue;
                        case ArgStoredOrInvokedAction.OverwriteOrReinvoke:
                            break;
                        case ArgStoredOrInvokedAction.ShowError:
                            Console.WriteLine($"Parameter '{args[i]}' is already invoked or already stored the value.");
                            return false;
                    }
                }

                switch (arg)
                {
                    case ArgStoreBase av:

                        if (av.IsSwitch)
                        {
                            av.Value = "true";
                            continue;
                        }

                        if ((i + 1) >= args.Length)
                        {
                            if (!av.IsOptional)
                            {
                                Console.WriteLine($"Value for '{args[i]}' is not found.");
                                ShowUsage();
                                return false;
                            }
                            continue;
                        }
                        i++;
                        av.Value = args[i];
                        continue;
                    // This switch will be remove later.
                    case ArgSwitch asw:
                        asw.Value = !asw.Value;
                        continue;
                    case ArgInvoke aInv:
                        aInv.Invoke();
                        continue;
                }
            }

            return true;
        }


        /// <summary>
        /// Gets the <see cref="ArgStoreBase"/> variables and it's values.
        /// </summary>
        /// <returns>Returns the array of <see cref="ArgStoreBase"/> classes.</returns>
        public static ArgStoreBase[] GetArgStoreValues()
        {
            List<ArgStoreBase> argStores = new List<ArgStoreBase>();
            foreach (RootArgument ar in args)
                if (ar is ArgStoreBase store)
                    argStores.Add(store);

            return argStores.ToArray();
        }

        /// <summary>
        /// Gets the typed value stored for the specified argument parameter name.
        /// </summary>
        /// <typeparam name="T">The expected type of the stored value (e.g. <see cref="string"/>, <see cref="int"/>, <see cref="bool"/>).</typeparam>
        /// <param name="paramName">The parameter name to look up (e.g. "--output").</param>
        /// <returns>
        /// The typed value associated with <paramref name="paramName"/>,
        /// or <c>default(T)</c> if no matching argument was found.
        /// </returns>
        public static T GetValue<T>(string paramName)
        {
            ArgStore<T> store = args
                .OfType<ArgStore<T>>()
                .SingleOrDefault(arg => arg.Parameters.Contains(paramName));

            return store != null ? store.TypedValue : default;
        }


        #region DEPRECATED METHODS. THIS WILL REMOVE IN THE FUTURE.

        /// <summary>
        /// Sets the argument to invoke a method if the parameter is matched.
        /// </summary>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="a">The provided <see cref="Action"/> method to be invoke later.</param>
        /// <param name="helpMsg">The help message.</param>
        /// <exception cref="ArgumentParseException"></exception>
        [Obsolete("This method name will be remove in the future. Use AddArgumentAction instead.")]
        public static void AddArgument(string[] parameters, Action a, string helpMsg = "")
        {
            if (a == null)
                throw new ArgumentParseException("Invoke not been given for the argument.");
            ArgInvoke argInvoke = new ArgInvoke(a)
            {
                Parameters = parameters,
                HelpMessage = helpMsg
            };
            InsertArgument(argInvoke);
        }

        /// <summary>
        /// Sets the argument to act as a switch if the parameter is matched.
        /// </summary>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="defSwitch">The default boolean switch.</param>
        /// <param name="helpMsg">The help message.</param>
        [Obsolete("This method will be remove in the future for clarity. Use AddArgument<T>")]
        public static void AddArgument(string[] parameters, bool defSwitch = false, string helpMsg = "")
        {
            ArgSwitch argSwitch = new ArgSwitch(defSwitch)
            {
                Parameters = parameters,
                HelpMessage = helpMsg
            };
            InsertArgument(argSwitch);
        }

        /// <summary>
        /// Sets the argument to store a value if the parameter is matched.
        /// </summary>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="valPlaceHoder">A placeholder of the specific value name.</param>
        /// <param name="helpMsg">The help message.</param>
        [Obsolete("This method is obsolete and it will be remove in the future. Use AddArgument<T>() instead")]
        public static void AddArgument(string[] parameters, string valPlaceHoder = "(paramName)", string helpMsg = "")
        {
            ArgStore<string> argStore = new ArgStore<string>()
            {
                Parameters = parameters,
                HelpMessage = helpMsg,
                ValueName = valPlaceHoder
            };
            InsertArgument(argStore);
        }

        /// <summary>
        /// Gets the <see cref="ArgSwitch"/> variables and it's values.
        /// </summary>
        /// <returns>Returns the array of <see cref="ArgSwitch"/> classes.</returns>
        [Obsolete("This method is obsolete and it will be remove in the future. Use GetValue<T>() instead.")]
        public static ArgSwitch[] GetArgSwitchValues()
        {
            List<ArgSwitch> argStores = new List<ArgSwitch>();
            foreach (RootArgument ar in args)
                if (ar is ArgSwitch store)
                    argStores.Add(store);

            return argStores.ToArray();
        }

        #endregion


        /// <summary>
        /// Invokes the help and exits..
        /// </summary>
        internal static void InvokeHelp()
        {
            ShowUsage();
            if (!string.IsNullOrWhiteSpace(description))
                Console.WriteLine(description + "\n");
            ShowHelp();
            if (!string.IsNullOrWhiteSpace(epilog))
                Console.WriteLine(epilog);
            if (examples.Any())
            {
                Console.WriteLine("\nExample/s:");
                foreach (string argExample in examples)
                {
                    Console.WriteLine($"\t{argExample}");
                }
            }
            Environment.Exit(0);
        }

        private static void InsertArgument(RootArgument arg)
        {
            if (arg == null || !arg.Parameters.Any() || arg.Parameters.Any(r => string.IsNullOrWhiteSpace(r) ||
                arg.Parameters.Distinct().Count() != arg.Parameters.Length ||
                args.Any(arx => IsParameterMatched(arg.Parameters, arx.Parameters))))
                throw new ArgumentParseException("Argument failed to parse or already added.");
            args.Add(arg);
        }

        /// <summary>
        /// Checks if the new parameter array contains parameter from the old ones.
        /// </summary>
        /// <param name="newParam">The new parameter array.</param>
        /// <param name="oldParam">The old parameter array.</param>
        /// <returns>
        /// Returns true if the new parameter array contains parameter from the old parameter array.
        /// Otherwise, false.
        /// </returns>
        private static bool IsParameterMatched(string[] newParam, string[] oldParam)
        {
            if (newParam.Length == 0 || oldParam.Length == 0) return false;
            foreach (string newP in newParam)
                if (oldParam.Contains(newP))
                    return true;
            return false;
        }

        /// <summary>
        /// Shows the help usage.
        /// </summary>
        private static void ShowUsage()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Count; i++)
            {
                RootArgument arg = args[i];
                sb.Append('[');
                for (int j = 0; j < arg.Parameters.Length; j++)
                {
                    sb.Append(arg.Parameters[j]);

                    if (arg is ArgStoreBase argStore)
                    {
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
        private static void ShowHelp()
        {
            List<string[]> helpArgs = new List<string[]>();
            foreach (RootArgument arg in args)
            {
                if (arg.Parameters != null)
                {
                    string[] argHelp = new string[2];
                    StringBuilder sb = new StringBuilder();

                    sb.Append("     ");
                    for (int i = 0; i < arg.Parameters.Length; i++)
                    {
                        sb.Append(arg.Parameters[i]);

                        if (arg is ArgStoreBase argStore && !argStore.IsSwitch)
                        {
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
        private static string GenerateTable(string[][] array, int padLength = 3)
        {
            StringBuilder sb = new StringBuilder();
            int rows = array.Length;
            int cols = array[0].Length;

            // Calculate column widths
            int[] columnWidths = new int[cols - 1]; // Exclude the last column

            for (int j = 0; j < cols - 1; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    int length = array[i][j].Length;
                    columnWidths[j] = Math.Max(columnWidths[j], length);
                }
            }

            for (int i = 0; i < rows; i++)
            {
                int restColLen = 0;
                for (int j = 0; j < cols - 1; j++)
                {
                    string val = array[i][j];
                    string padded = new string(' ', columnWidths[j] + padLength - val.Length);
                    sb.Append($"{val}{padded}");
                    restColLen += val.Length + padded.Length;
                }
                int resCol = Console.WindowWidth - restColLen;

                string paragraph = array[i][cols - 1];

                if (resCol <= 0)
                {
                    sb.AppendLine(paragraph);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(paragraph))
                {
                    sb.AppendLine(" ");
                    continue;
                }
                if (paragraph.Length <= resCol && !paragraph.Contains("\n") && !paragraph.Contains("\r\n"))
                {
                    sb.AppendLine($"{paragraph}");
                    continue;
                }
                string[] splitString = paragraph.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
                List<string> lines = new List<string>();
                foreach (string line in splitString)
                {
                    string modLine = line;
                    while (modLine.Length > resCol && resCol > 0)
                    {
                        lines.Add(modLine.Substring(0, resCol));
                        modLine = modLine.Remove(0, resCol);
                    }
                    lines.Add(modLine);
                }

                bool firstIter = false;
                foreach (string line in lines)
                {
                    sb.AppendLine($"{(!firstIter ? "" : new string(' ', restColLen))}{line}");
                    if (!firstIter) firstIter = true;
                }
            }
            return sb.ToString();
        }
    }
}
