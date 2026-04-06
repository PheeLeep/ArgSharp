using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PheeLeep.ArgSharp;
using PheeLeep.ArgSharp.Args;
using static ArgSharp.ArgSharpClass;

namespace ArgSharp.Args
{

    /// <summary>
    /// An argument class that allows to insert an <see cref="Action"/> method
    /// and invoke when the parameter specified to this class is matched.
    /// </summary>
    public class ArgInvoke : RootArgument
    {

        private readonly Action a;

        private readonly List<RootArgument> args = new List<RootArgument>();
        private readonly List<string> examples = new List<string>();
        private bool subcommandWasInvoked = false;

        private string description;
        private string epilog;

        /// <summary>
        /// Gets the parent <see cref="ArgInvoke"/> node. Returns null if it's a root node.
        /// </summary>
        internal ArgInvoke ParentNode { get; private set; }

        /// <summary>
        /// Gets or sets the parser's behavior if the commandline argument is zero.<br />
        /// Not changing this property will follow <see cref="ArgSharpClass"/>'s own value of .
        /// </summary>
        public ArgZeroAction ArgumentZeroAction { get; set; } = ArgSharpClass.ArgumentZeroAction;

        /// <summary>
        /// Sets the argument to store a specific variable.
        /// </summary>
        /// <typeparam name="T">The specific variable type.</typeparam>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="placeHolder">A placeholder of the specific value name.</param>
        /// <param name="helpMsg">The help message.</param>
        /// <param name="defaultValue">The default value for the specified type.</param>
        public void AddArgument<T>(string[] parameters,
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
        public void AddExample(string example)
        {
            if (string.IsNullOrWhiteSpace(example)) return;
            examples.Add(example);
        }

        /// <summary>
        /// Sets the argument to invoke a method if the parameter is matched.
        /// </summary>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="a">
        /// The provided <see cref="Action"/> method to be invoke later.<br /> 
        /// Leaving it null will run help message if no succeeding arguments supplied.
        /// </param>
        /// <param name="helpMsg">The help message.</param>
        /// <param name="description">The description of the program.</param>
        /// <param name="epilog">The epilogue of the program after printing the help.</param>
        /// <returns>Returns the <see cref="ArgInvoke"/> object.</returns>
        /// <exception cref="ArgumentParseException"></exception>
        public ArgInvoke AddArgumentAction(string[] parameters,
                                           Action a = null,
                                           string helpMsg = "",
                                           string description = "",
                                           string epilog = "")
        {
            ArgInvoke argInvoke = new ArgInvoke(a, this, description, epilog)
            {
                Parameters = parameters,
                HelpMessage = helpMsg
            };
            InsertArgument(argInvoke);

            return argInvoke;
        }

        /// <summary>
        /// Gets the <see cref="ArgStoreBase"/> variables and it's values.
        /// </summary>
        /// <returns>Returns the array of <see cref="ArgStoreBase"/> classes.</returns>
        public ArgStoreBase[] GetArgStoreValues()
        {
            List<ArgStoreBase> argStores = new List<ArgStoreBase>();
            foreach (RootArgument ar in args)
                if (ar is ArgStoreBase store)
                    argStores.Add(store);

            return argStores.ToArray();
        }

        /// <summary>
        /// Instantiate the <see cref="ArgInvoke"/> class.
        /// </summary>
        /// <param name="a">An <see cref="Action"/> method to be invoke later. Leaving it null will run the help message.</param>
        /// <param name="parent">The parent node.</param>
        /// <param name="description">The description of the program.</param>
        /// <param name="epilog">The epilogue of the program after printing the help.</param>
        internal ArgInvoke(Action a,
                           ArgInvoke parent = null,
                           string description = "",
                           string epilog = "") : base()
        {
            ParentNode = parent;
            this.a = a;
            this.description = description;
            this.epilog = epilog;
        }

        /// <summary>
        /// Invoke the specified <see cref="Action"/> stored to the class.
        /// </summary>
        /// <param name="args">An array of remaining arguments.</param>
        /// <param name="statusCode">Returns the status code. (BETA)</param>
        /// <param name="errorOutput">The text writer to output error messages.</param>
        internal bool Invoke(string[] args, out int statusCode, TextWriter errorOutput = null)
        {
            statusCode = 0;
            if (IsArgStoredOrInvoked) return false;
            if (!Parse(args, out statusCode, errorOutput)) return false;
            SetArgStoredOrInvoked();

            // If a subcommand was already invoked during Parse, don't invoke action or show help
            if (subcommandWasInvoked) return true;

            if (a is null)
            {
                ShowHelp();
            }
            else
            {
                a();
            }
            return true;
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
        public T GetValue<T>(string paramName)
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

        private bool Parse(string[] args, out int statusCode, TextWriter errorOutput = null)
        {
            statusCode = 0;
            if (errorOutput == null) errorOutput = Console.Error;
            if (args.Length == 0)
            {
                switch (ArgumentZeroAction)
                {
                    case ArgZeroAction.TreatAsSuccess:
                        return true;
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

                var argTxt = args[i];
                // Check for help flag before processing arguments
                if (argTxt.ToLower() == "-h" || argTxt.ToLower() == "--help")
                {
                    InvokeHelp();
                    return false;
                }

                RootArgument arg = this.args.FirstOrDefault(r => r.Parameters.Contains(argTxt));
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
                    case ArgInvoke aInv:
                        // Pass only remaining args after the subcommand name to the subcommand
                        string[] remainingArgs = args.Skip(i + 1).ToArray();
                        subcommandWasInvoked = true;
                        bool invokeResult = aInv.Invoke(remainingArgs, out statusCode, errorOutput);
                        return invokeResult;  // Return the result of subcommand invocation
                }
            }

            List<string> missingRequired = new List<string>();
            foreach (RootArgument arg in this.args)
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
        private void InsertArgument(RootArgument arg)
        {
            if (ArgSharpClass.IsParsed)
                throw new InvalidOperationException("Already parsed. Unable to add another argument.");

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
        /// Invokes the help.
        /// </summary>
        internal void InvokeHelp()
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

        /// <summary>
        /// Shows the help usage.
        /// </summary>
        private void ShowUsage()
        {
            if (!string.IsNullOrWhiteSpace(title))
                Console.WriteLine(title + "\n");

            int consoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;

            // Build each argument token e.g. [-h | --help] or [-o <file>]
            List<string> tokens = new List<string>();
            if (args.Any(a => a is ArgInvoke))
            {
                tokens.Add("{action}");
            }

            foreach (RootArgument arg in args)
            {
                if (arg is ArgInvoke) continue;
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

            // Since help does not included, it should be added.
            tokens.Add("[-h | --help]");

            // Print "Usage:" header then wrap tokens across lines
            string usageLabel = $"  {progName} {GetSubCommandNamePath()} ";
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

        private string GetSubCommandNamePath()
        {
            string res = "";
            if (ParentNode != null)
            {
                string parentPath = ParentNode.GetSubCommandNamePath();
                if (!string.IsNullOrEmpty(parentPath))
                    res += parentPath + " ";
            }

            if (Parameters != null && Parameters.Length > 0)
            {
                res += $"{Parameters[0]}";
            }

            return res;
        }
        /// <summary>
        /// Shows the help description.
        /// </summary>
        private void ShowHelp()
        {
            List<string[]> helpArgs = new List<string[]>();
            List<string[]> actionArgs = new List<string[]>();

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

                if (arg is ArgInvoke)
                {
                    actionArgs.Add(new[] { sb.ToString(), arg.HelpMessage ?? "" });
                    continue;
                }
                helpArgs.Add(new[] { sb.ToString(), arg.HelpMessage ?? "" });
            }

            // Add help option
            helpArgs.Add(new[] { "  -h, --help", "Show this message and exit." });

            if (actionArgs.Any())
            {
                Console.WriteLine("Actions:");
                Console.WriteLine(Miscellaneous.GenerateTable(actionArgs.ToArray()));
            }

            Console.WriteLine("Options:");
            Console.WriteLine(Miscellaneous.GenerateTable(helpArgs.ToArray()));
        }

    }
}
