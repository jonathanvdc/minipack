using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Flame.Compiler;
using Flame.Front.Options;

namespace Minipack
{
    /// <summary>
    /// A subprogram that creates a directory tree containing all files from a package.
    /// </summary>
    public sealed class FilesSubprogram : Subprogram
    {
        private FilesSubprogram()
        { }

        /// <summary>
        /// An instance of a files subprogram.
        /// </summary>
        public static readonly FilesSubprogram Instance = new FilesSubprogram();

        /// <inheritdoc/>
        public override string Description => "Creates a directory tree to which all files are copied.";

        /// <inheritdoc/>
        public override string Usage => "minipack.json [--source-dir /path/to/source/] [-o /path/to/output/]";

        /// <inheritdoc/>
        public override void Run(IReadOnlyList<string> args, ICompilerLog log)
        {
            var optParser = StringOptionParser.CreateDefault();
            var options = BuildArguments.Parse(optParser, args.ToArray<string>());
            log = new OptionLog(log, options);

            if (options.SourcePaths.Length != 1)
            {
                LogInvalidSourcePathArity(log, options.SourcePaths.Length);
                return;
            }

            string sourcePath = options.SourcePaths[0].ToString();
            var packageDesc = PackageDescription.Read(sourcePath);

            string outputDir = options.GetOption<string>("o", null);
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = packageDesc.Name + "-files";
            }

            string sourceDir = options.GetOption<string>("source-dir", null);
            if (string.IsNullOrWhiteSpace(sourceDir))
            {
                sourceDir = Environment.CurrentDirectory;
            }

            PopulateTarget(
                log,
                packageDesc,
                sourceDir,
                outputDir);
        }

        private static void PopulateTarget(
            ICompilerLog log,
            PackageDescription package,
            string sourceDirectory,
            string targetDirectory)
        {
            var namedPaths = new Dictionary<string, string>();
            namedPaths[TargetDescription.ExecutableDirectoryPathName] = ".";

            // Copy files to the directory.
            package.CopyFilesToTarget(sourceDirectory, new TargetDescription(targetDirectory, namedPaths));
        }

        /// <summary>
        /// Writes an error to the log which tells the user that they failed
        /// to provide exactly one source path.
        /// </summary>
        /// <param name="log">The log to write an error to.</param>
        /// <param name="arity">The number of paths provided by the user.</param>
        public static void LogInvalidSourcePathArity(ICompilerLog log, int arity)
        {
            if (arity > 1)
            {
                log.LogError(
                    new LogEntry(
                        "too many input files",
                        string.Format(
                            "exactly one input file was expected, but {0} were provided.",
                            arity)));
            }
            else
            {
                log.LogError(
                    new LogEntry(
                        "no input file",
                        "exactly one input file must be provided."));
            }
        }
    }
}

