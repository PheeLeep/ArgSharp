using System;
using System.Collections.Generic;
using PheeLeep.ArgSharp.Args;

namespace ArgSharp.Args
{

    /// <summary>
    /// A class that stores string information after the specified parameter.
    /// </summary>
    public class ArgStore<T> : ArgStoreBase
    {


        // These are the list of supported variable types.
        private static readonly Dictionary<Type, Func<string, object>> Parsers
            = new Dictionary<Type, Func<string, object>>
        {
                { typeof(string),  v => v },
                { typeof(int),     v => int.Parse(v) },
                { typeof(long),    v => long.Parse(v) },
                { typeof(float),   v => float.Parse(v) },
                { typeof(double),  v => double.Parse(v) },
                { typeof(decimal), v => decimal.Parse(v) },
                { typeof(bool),    v => bool.Parse(v) }
        };


        private T typedValue;

        private string stringValue = "";

        /// <summary>
        /// Instantiate the <see cref="ArgStore"/> class.
        /// </summary>
        /// <exception cref="ArgumentParseException"></exception>
        internal ArgStore() : base()
        {
            var type = typeof(T);
            if (!Parsers.ContainsKey(type))
                throw new ArgumentParseException($"Type {type.Name} is not supported.");

            typedValue = default;
            IsOptional = false;
        }

        /// <summary>
        /// Instantiate the <see cref="ArgStore"/> class with a default value.
        /// </summary>
        /// <param name="defaultValue">The default value</param>
        internal ArgStore(T defaultValue = default) : base()
        {
            var type = typeof(T);
            if (!Parsers.ContainsKey(type))
                throw new ArgumentParseException($"Type {type.Name} is not supported.");
            typedValue = defaultValue;
            IsOptional = true;
        }

        public override string Value
        {
            get => stringValue;
            internal set
            {
                if (string.IsNullOrEmpty(value) || IsArgStoredOrInvoked) return;
                stringValue = value;
                typedValue = ConvertValue(value);
                SetArgStoredOrInvoked();
            }
        }

        /// <summary>
        /// Gets the typed value.
        /// </summary>
        public T TypedValue => typedValue;

        public override object GetTypedValue()
        {
            return typedValue;
        }

        public override bool IsSwitch { get => typeof(T) == typeof(bool); }

        /// <summary>
        /// Converts the plain string value into type specific.
        /// </summary>
        /// <param name="value">A string value.</param>
        /// <returns>Returns a T specific value.</returns>
        /// <exception cref="ArgumentParseException"></exception>
        private T ConvertValue(string value)
        {
            return (T)Parsers[typeof(T)](value);
        }
    }
}
