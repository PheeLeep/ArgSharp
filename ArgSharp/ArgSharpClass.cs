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
        /// <summary>
        /// A main ArgInvoke for the ArgSharpClass.
        /// </summary>
        private static ArgInvoke motherArg = null;

        /// <summary>
        /// Gets the last status code. 
        /// </summary>
        public static int StatusCode { get; private set; } = 0;

        internal static string progName;
        internal static string description;
        internal static string epilog;
        internal static string title;

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
        /// Gets if <see cref="ArgSharpClass"/> is already parsed.
        /// </summary>
        public static bool IsParsed { get; internal set; } = false;

        /// <summary>
        /// Initializes the parser.
        /// </summary>
        /// <param name="progName">The name of the program. (example: ArgSharpCmd.exe or ArgSharpCmd)</param>
        /// <param name="title">The title of the program.</param>
        /// <param name="description">The description of the program.</param>
        /// <param name="epilog">The epilogue of the program after printing the help.</param>
        public static void Init(string progName, string title = "", string description = "", string epilog = "")
        {
            if (motherArg != null)
            {
                Console.WriteLine("Argument parser already initialized.");
                return;
            }

            motherArg = new ArgInvoke(null);
            ArgSharpClass.progName = progName;
            ArgSharpClass.description = description;
            ArgSharpClass.epilog = epilog;
            ArgSharpClass.title = title;
        }

        /// <summary>
        /// Sets the argument to store a specific variable.
        /// </summary>
        /// <typeparam name="T">The specific variable type.</typeparam>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="placeHolder">A placeholder of the specific value name.</param>
        /// <param name="helpMsg">The help message.</param>
        /// <param name="defaultValue">The default value for the specified type.</param>
        /// <param name="isRequired">Sets if the specific argument is required.</param>
        public static void AddArgument<T>(string[] parameters,
                                          string placeHolder = "(value)",
                                          string helpMsg = "",
                                          T defaultValue = default,
                                          bool isRequired = false)
        {
            if (motherArg is null)
            {
                throw new InvalidOperationException($"Initialization has not been made. Invoke '{nameof(Init)}' first.");
            }

            motherArg.AddArgument(parameters, placeHolder, helpMsg, defaultValue, isRequired);
        }

        /// <summary>
        /// Inserts the example of using your program with a specific parameter.
        /// </summary>
        /// <param name="example">The example string.</param>
        public static void AddExample(string example)
        {
            if (string.IsNullOrWhiteSpace(example)) return;
            if (motherArg is null)
            {
                throw new InvalidOperationException($"Initialization has not been made. Invoke '{nameof(Init)}' first.");
            }

            motherArg.AddExample(example);
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
        /// <returns>Returns the <see cref="ArgInvoke"/> object.</returns>
        /// <exception cref="ArgumentParseException"></exception>
        public static ArgInvoke AddArgumentAction(string[] parameters, Action a = null, string helpMsg = "")
        {
            if (motherArg is null)
            {
                throw new InvalidOperationException($"Initialization has not been made. Invoke '{nameof(Init)}'");
            }
            return motherArg.AddArgumentAction(parameters, a, helpMsg);
        }

        /// <summary>
        /// Gets the <see cref="ArgStoreBase"/> variables and its values on <see cref="ArgSharpClass" />
        /// </summary>
        /// <returns>Returns the array of <see cref="ArgStoreBase"/> classes.</returns>
        public static ArgStoreBase[] GetArgStoreValues()
        {
            if (motherArg is null)
            {
                throw new InvalidOperationException($"Initialization has not been made. Invoke '{nameof(Init)}'");
            }
            return motherArg.GetArgStoreValues();
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
        /// <returns>Returns a boolean if parsing has been completed.</returns>
        /// <exception cref="InvalidOperationException />
        public static bool Parse(string[] args, TextWriter errorOutput = null)
        {
            if (motherArg is null)
            {
                throw new InvalidOperationException("ArgSharpClass is not initiated.");
            }
            if (IsParsed)
            {
                throw new InvalidOperationException("Already parsed.");
            }

            IsParsed = true;
            var res = motherArg.Invoke(args, out int statusCode, errorOutput);
            StatusCode = statusCode;
            return res;
        }
    }
}
