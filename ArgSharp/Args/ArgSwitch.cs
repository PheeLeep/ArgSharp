namespace ArgSharp.Args {

    /// <summary>
    /// A class that stores a <see cref="bool"/> information and sets to true
    /// when the argument parameter matched to the specified parameter to the class.
    /// </summary>
    public class ArgSwitch : RootArgument {

        /// <summary>
        /// Instantiate the <see cref="ArgSwitch"/> class.
        /// </summary>
        internal ArgSwitch() : base() { }

        /// <summary>
        /// Gets the <see cref="bool"/> value.
        /// </summary>
        public bool Value { get; internal set; }
    }
}
