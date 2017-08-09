using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flame.Compiler;
using Flame.Front.Options;

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
            var optParser = StringOptionParser.CreateDefault();
            var options = BuildArguments.Parse(optParser, args.ToArray<string>());
            log = new OptionLog(log, options);

            if (options.SourcePaths.Length != 1)
            {
                LogInvalidSourcePathArity(log, options.SourcePaths.Length);
                return;
            }

            string sourcePath = options.SourcePaths[0].ToString();

            string version = options.GetOption<string>("version", null);
            if (string.IsNullOrWhiteSpace(version))
            {
                LogMissingMandatoryOption(log, "--version");
                return;
            }

            string revision = options.GetOption<string>("revision", null);
            if (string.IsNullOrWhiteSpace(revision))
            {
                LogMissingMandatoryOption(log, "--revision");
                return;
            }

            string outputDir = options.GetOption<string>("o", null);
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                LogMissingMandatoryOption(log, "-o");
                return;
            }

            var packageDesc = PackageDescription.Read(sourcePath);

            Dictionary<string, string> debConfig = packageDesc.GetConfigOrNull("deb");
            if (debConfig == null)
            {
                LogMissingDebTarget(log, sourcePath);
                return;
            }

            string partialControlPath;
            if (!debConfig.TryGetValue("control", out partialControlPath))
            {
                LogMissingControlPath(log, sourcePath);
                return;
            }

            var partialControl = File.ReadAllText(partialControlPath);

            PopulateTarget(packageDesc, partialControl, version, revision, outputDir);
        }

        private static void PopulateTarget(
            PackageDescription package,
            string partialControl,
            string version,
            string revision,
            string targetDirectory)
        {
            // Create the DEBIAN/control file.
            var controlBuilder = new StringBuilder();
            controlBuilder.Append("Package: " + package.Name + "\n");
            controlBuilder.Append("Version: " + version + "-" + revision + "\n");
            controlBuilder.Append(partialControl);

            var debianDirPath = Path.Combine(targetDirectory, "DEBIAN");

            Directory.CreateDirectory(debianDirPath);

            File.WriteAllText(Path.Combine(debianDirPath, "control"), controlBuilder.ToString());

            // Copy files to the usr directory.
            package.CopyFilesToTarget(Environment.CurrentDirectory, Path.Combine(targetDirectory, "usr"));

            // Instantiate executables.
            
        }

        private static void LogMissingMandatoryOption(ICompilerLog log, string optionName)
        {
            log.LogError(
                new LogEntry(
                    "missing mandatory option",
                    string.Format(
                        "the mandatory '{0}' option is missing.",
                        optionName)));
        }

        private static void LogInvalidSourcePathArity(ICompilerLog log, int arity)
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

        private static void LogMissingDebTarget(ICompilerLog log, string sourcePath)
        {
            log.LogError(
                new LogEntry(
                    "missing 'deb' target",
                    string.Format(
                        "debian packages are generated based on the properties " +
                        "of the 'deb' target, but the file at '{0}' doesn't have one.",
                        sourcePath)));
        }

        private static void LogMissingControlPath(ICompilerLog log, string sourcePath)
        {
            log.LogError(
                new LogEntry(
                    "missing 'control' property",
                    string.Format(
                        "the 'deb' target of the file at '{0}' must have a 'control' " +
                        "property that points to a partial control file.",
                        sourcePath)));
        }
    }
}

