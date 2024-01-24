namespace ArgSharp.Args {

    /// <summary>
    /// An abstract class use for implementing derived classes for other purposes
    /// regarding commandline argument parsinng.
    /// </summary>
    public abstract class RootArgument {

        /// <summary>
        /// Gets the list of the specified parameters for the class.
        /// </summary>
        public string[] Parameters { get; internal set; }

        /// <summary>
        /// Gets the help message.
        /// </summary>
        internal string HelpMessage { get; set; }
    }
}
