using ArgSharp.Args;
using PheeLeep.ArgSharp.Args;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// Gets or sets the method to be invoke when the help is invoked.
        /// </summary>
        public static Action OnHelpInvoked { get; set; } = new Action(() => { Environment.Exit(0); });

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
                                          T defaultValue = default,
                                          bool isRequired = false)
        {
            var argStore = new ArgStore<T>(defaultValue)
            {
                Parameters = parameters,
                HelpMessage = helpMsg,
                ValueName = placeHolder,
                IsRequired = isRequired
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
        /// <param name="errorOutput">The text writer to output error messages.</param>
        public static bool Parse(TextWriter errorOutput = null)
        {
            List<string> argSpecified = new List<string>(Environment.GetCommandLineArgs());
            argSpecified.RemoveAt(0);
            return Parse(argSpecified.ToArray(), errorOutput);
        }

        /// <summary>
        /// Parses the arguments specified on the parameter.
        /// </summary>
        /// <param name="args">An array of string arguments.</param>
        /// <param name="errorOutput">The text writer to output error messages.</param>
        public static bool Parse(string[] args, TextWriter errorOutput = null)
        {
            if (ArgSharpClass.args.Count == 0) return false;
            if (errorOutput == null) errorOutput = Console.Error;
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
                        ShowUsage();
                        errorOutput.WriteLine("No argument has been given.");
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
                    errorOutput.WriteLine($"Parameter '{args[i]}' is not found.");
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
                            errorOutput.WriteLine($"Parameter '{args[i]}' is already invoked or already stored the value.");
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
                            if (av.IsRequired)
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

            List<string> missingRequired = new List<string>();
            foreach (RootArgument arg in ArgSharpClass.args)
            {
                if (arg is ArgStoreBase store && store.IsRequired && !arg.IsArgStoredOrInvoked)
                    missingRequired.Add(string.Join(" | ", arg.Parameters));
            }

            if (missingRequired.Count > 0)
            {
                ShowUsage();
                errorOutput.WriteLine($"The following required argument(s) were not provided: " +
                                  $"{string.Join(", ", missingRequired)}");
                return false;
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

            ArgStoreBase match = args
                   .OfType<ArgStoreBase>()
                   .SingleOrDefault(arg => arg.Parameters.Contains(paramName));

            if (match == null)
                return default;

            if (match is ArgStore<T> typedStore)
                return typedStore.TypedValue;

            throw new InvalidCastException(
    $"Argument '{paramName}' was registered as '{match.GetTypedValue()?.GetType().Name ?? "unknown"}'" +
    $", but GetValue was called with type '{typeof(T).Name}'.");
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
            OnHelpInvoked();
        }

        private static void InsertArgument(RootArgument arg)
        {
            if (arg == null)
                throw new ArgumentParseException("Argument cannot be null.");

            if (arg.Parameters == null || !arg.Parameters.Any())
                throw new ArgumentParseException("Argument must have at least one parameter (e.g. \"-o\" or \"--output\").");

            if (arg.Parameters.Any(p => string.IsNullOrWhiteSpace(p)))
                throw new ArgumentParseException("Argument parameters cannot be null or whitespace.");

            if (arg.Parameters.Distinct().Count() != arg.Parameters.Length)
            {
                // Find and name the actual duplicate for a helpful message.
                string duplicate = arg.Parameters
                    .GroupBy(p => p)
                    .First(g => g.Count() > 1)
                    .Key;
                throw new ArgumentParseException(
                    $"Duplicate parameter detected within the same argument: '{duplicate}'.");
            }

            foreach (RootArgument existing in args)
            {
                string conflict = arg.Parameters.FirstOrDefault(p => existing.Parameters.Contains(p));
                if (conflict != null)
                    throw new ArgumentParseException(
                        $"Parameter '{conflict}' is already registered by another argument.");
            }

            args.Add(arg);
        }

        /// <summary>
        /// Shows the help usage.
        /// </summary>
        private static void ShowUsage()
        {
            if (!string.IsNullOrWhiteSpace(title))
                Console.WriteLine(title + "\n");

            int consoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;

            // Build each argument token e.g. [-h | --help] or [-o <file>]
            List<string> tokens = new List<string>();
            foreach (RootArgument arg in args)
            {
                StringBuilder sb = new StringBuilder("[");
                for (int j = 0; j < arg.Parameters.Length; j++)
                {
                    sb.Append(arg.Parameters[j]);

                    if (arg is ArgStoreBase argStore && !argStore.IsSwitch
                        && !string.IsNullOrWhiteSpace(argStore.ValueName))
                    {
                        sb.Append($" <{argStore.ValueName}>");
                    }

                    if ((j + 1) < arg.Parameters.Length)
                        sb.Append(" | ");
                }
                sb.Append(']');
                tokens.Add(sb.ToString());
            }

            // Print "Usage:" header then wrap tokens across lines
            string usageLabel = $"  {progName} ";
            int indent = usageLabel.Length;
            Console.WriteLine("Usage:");
            Console.Write(usageLabel);

            int currentLineLen = indent;
            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i] + (i < tokens.Count - 1 ? " " : "");

                // If token doesn't fit on current line, wrap
                if (currentLineLen + token.Length > consoleWidth && currentLineLen > indent)
                {
                    Console.WriteLine();
                    Console.Write(new string(' ', indent));
                    currentLineLen = indent;
                }

                Console.Write(token);
                currentLineLen += token.Length;
            }

            Console.WriteLine("\n");
        }


        /// <summary>
        /// Shows the help description.
        /// </summary>
        private static void ShowHelp()
        {
            List<string[]> helpArgs = new List<string[]>();

            foreach (RootArgument arg in args)
            {
                if (arg.Parameters == null) continue;

                StringBuilder sb = new StringBuilder("  ");
                for (int i = 0; i < arg.Parameters.Length; i++)
                {
                    sb.Append(arg.Parameters[i]);

                    // Only show value placeholder for non-switch stores with a non-empty name
                    if (arg is ArgStoreBase argStore && !argStore.IsSwitch
                        && !string.IsNullOrWhiteSpace(argStore.ValueName))
                    {
                        sb.Append($" <{argStore.ValueName}>");
                    }

                    if ((i + 1) < arg.Parameters.Length)
                        sb.Append(", ");
                }

                helpArgs.Add(new[] { sb.ToString(), arg.HelpMessage ?? "" });
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
        private static string GenerateTable(string[][] array, int padLength = 4)
        {
            if (array == null || array.Length == 0) return string.Empty;

            int consoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;
            StringBuilder sb = new StringBuilder();

            // Find the widest left column
            int leftColWidth = 0;
            foreach (string[] row in array)
                if (row[0].Length > leftColWidth)
                    leftColWidth = row[0].Length;

            int rightColStart = leftColWidth + padLength;

            // Cap rightColStart so the right column always has at least 30 chars
            if (rightColStart > consoleWidth - 30)
                rightColStart = consoleWidth - 30;

            int rightColWidth = consoleWidth - rightColStart;

            foreach (string[] row in array)
            {
                string left = row[0];
                string right = row.Length > 1 ? (row[1] ?? "") : "";

                // Pad left column
                string leftPadded = left.PadRight(rightColStart);

                if (string.IsNullOrWhiteSpace(right))
                {
                    sb.AppendLine(leftPadded.TrimEnd());
                    continue;
                }

                // Word-wrap the right column
                List<string> wrappedLines = WordWrap(right, rightColWidth);

                for (int i = 0; i < wrappedLines.Count; i++)
                {
                    if (i == 0)
                        sb.AppendLine($"{leftPadded}{wrappedLines[i]}");
                    else
                        sb.AppendLine($"{new string(' ', rightColStart)}{wrappedLines[i]}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Wraps text at word boundaries for a given max width.
        /// Respects explicit newlines (\n) in the source text.
        /// </summary>
        private static List<string> WordWrap(string text, int maxWidth)
        {
            List<string> result = new List<string>();
            if (maxWidth <= 0)
            {
                result.Add(text);
                return result;
            }

            // Split on explicit newlines first
            string[] hardLines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (string hardLine in hardLines)
            {
                if (hardLine.Length <= maxWidth)
                {
                    result.Add(hardLine);
                    continue;
                }

                // Word-wrap the line
                string[] words = hardLine.Split(' ');
                StringBuilder line = new StringBuilder();

                foreach (string word in words)
                {
                    if (line.Length == 0)
                    {
                        line.Append(word);
                    }
                    else if (line.Length + 1 + word.Length <= maxWidth)
                    {
                        line.Append(' ');
                        line.Append(word);
                    }
                    else
                    {
                        result.Add(line.ToString());
                        line.Clear();
                        line.Append(word);
                    }
                }

                if (line.Length > 0)
                    result.Add(line.ToString());
            }

            return result;
        }
    }
}
