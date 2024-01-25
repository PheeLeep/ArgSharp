namespace ArgSharp.Args {

    /// <summary>
    /// A class that stores string information after the specified parameter.
    /// </summary>
    public class ArgStore : RootArgument {

        private string valueName = "";

        /// <summary>
        /// Instantiate the <see cref="ArgStore"/> class.
        /// </summary>
        internal ArgStore() : base() { }

        /// <summary>
        /// Stores the information specified after the argument parameter specified to the class.
        /// </summary>
        public string Value {
            get => valueName;
            internal set {
                if (string.IsNullOrEmpty(value) || IsArgStoredOrInvoked) return;
                SetArgStoredOrInvoked();
                valueName = value;
            }
        }

        /// <summary>
        /// Gets the specified value name for the parameter.
        /// </summary>
        public string ValueName { get; internal set; }

    }
}
