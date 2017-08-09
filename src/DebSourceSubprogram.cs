using System;
using System.Collections.Generic;
using Flame.Compiler;

namespace Minipack
{
    /// <summary>
    /// A subprogram that creates a directory tree which can be packaged into a .deb by dpkg-deb.
    /// </summary>
    public sealed class DebSourceSubprogram : Subprogram
    {
        private DebSourceSubprogram()
        { }

        /// <summary>
        /// An instance of a debian-source subprogram.
        /// </summary>
        public static readonly DebSourceSubprogram Instance = new DebSourceSubprogram();

        /// <inheritdoc/>
        public override string Description => "Creates a directory tree that can be packaged into a .deb by dpkg-deb.";

        /// <inheritdoc/>
        public override string Usage => "minipack.json --version x.x.x.x --revision xxxx -o output-dir";

        /// <inheritdoc/>
        public override void Run(IReadOnlyList<string> args, ICompilerLog log)
        {
            throw new NotImplementedException();
        }
    }
}

