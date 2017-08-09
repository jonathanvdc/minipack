using System.Collections.Generic;
using Flame.Compiler;

namespace Minipack
{
    /// <summary>
    /// A minipack sub-program.
    /// </summary>
    public abstract class Subprogram
    {
        /// <summary>
        /// Gets the description for this subprogram.
        /// </summary>
        /// <returns>A human-readable description.</returns>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the usage for this subprogram.
        /// </summary>
        /// <returns>A usage description.</returns>
        public abstract string Usage { get; }

        /// <summary>
        /// Rusn this subprogram with the given arguments.
        /// </summary>
        /// <param name="args">The subprogram's arguments.</param>
        /// <param name="log">The log to use for user interaction.</param>
        public abstract void Run(IReadOnlyList<string> args, ICompilerLog log);
    }
}