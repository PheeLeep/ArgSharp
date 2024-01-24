namespace ArgSharp.Args {

    /// <summary>
    /// A class that stores string information after the specified parameter.
    /// </summary>
    public class ArgStore : RootArgument {

        /// <summary>
        /// Instantiate the <see cref="ArgStore"/> class.
        /// </summary>
        internal ArgStore() : base() { }

        /// <summary>
        /// Stores the information specified after the argument parameter specified to the class.
        /// </summary>
        public string Value { get; internal set; }

        /// <summary>
        /// Gets the specified value name for the parameter.
        /// </summary>
        public string ValueName { get; internal set; }

    }
}
