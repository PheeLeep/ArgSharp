using System;

namespace ArgSharp.Args {

    /// <summary>
    /// An argument class that allows to insert an <see cref="Action"/> method
    /// and invoke when the parameter specified to this class is matched.
    /// </summary>
    public class ArgInvoke : RootArgument {

        private readonly Action a;

        /// <summary>
        /// Instantiate the <see cref="ArgInvoke"/> class.
        /// </summary>
        /// <param name="a">An <see cref="Action"/> method to be invoke later.</param>
        internal ArgInvoke(Action a) : base() {
            this.a = a;
        }

        /// <summary>
        /// Invoke the specified <see cref="Action"/> stored to the class.
        /// </summary>
        internal void Invoke() {
            if (IsArgStoredOrInvoked) return;
            SetArgStoredOrInvoked();
            a();
        }
    }
}
