using System;

namespace ArgSharp {
    /// <summary>
    /// An <see cref="Exception"/>-derived class occurs when the argument parsing fail.
    /// </summary>
    public class ArgumentParseException : Exception {
        public ArgumentParseException(string message = "Argument parse failure.") : base(message) { }
    }
}
