using System;
using ArgSharp.Args;

namespace PheeLeep.ArgSharp.Args
{
    public abstract class ArgStoreBase : RootArgument
    {

        /// <summary>
        /// Gets the specified value name for the parameter.
        /// </summary>
        public string ValueName { get; internal set; }

        /// <summary>
        /// Allow the argument store to be optional.
        /// </summary>
        public bool IsOptional { get; internal set; }

        /// <summary>
        /// Gets if the argument type is a switch. (applicable to boolean)
        /// </summary>
        public abstract bool IsSwitch { get; }

        /// <summary>
        /// Stores the raw string value.
        /// </summary>
        public abstract string Value { get; internal set; }

        /// <summary>
        /// Returns the typed value as a boxed object.
        /// </summary>
        public abstract object GetTypedValue();
    }

}

