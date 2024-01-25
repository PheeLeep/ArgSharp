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

        /// <summary>
        /// Gets if the argument was invoked or stored a value.
        /// </summary>
        internal bool IsArgStoredOrInvoked { get; private set; }

        /// <summary>
        /// A derived-class must invoke this if the value was invoked or stored.
        /// </summary>
        protected virtual void SetArgStoredOrInvoked() {
            IsArgStoredOrInvoked = true;
        }
    }
}
