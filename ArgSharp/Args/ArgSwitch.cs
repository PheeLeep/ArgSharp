namespace ArgSharp.Args {

    /// <summary>
    /// A class that stores a <see cref="bool"/> information and sets to true
    /// when the argument parameter matched to the specified parameter to the class.
    /// </summary>
    public class ArgSwitch : RootArgument {

        private bool value;
        /// <summary>
        /// Instantiate the <see cref="ArgSwitch"/> class.
        /// </summary>
        internal ArgSwitch(bool presetValue = false) : base() {
            value = presetValue;
        }

        /// <summary>
        /// Gets the <see cref="bool"/> value.
        /// </summary>
        public bool Value {
            get => value;
            internal set {
                if (IsArgStoredOrInvoked) return;
                SetArgStoredOrInvoked();
                this.value = value;
            }
        }
    }
}
