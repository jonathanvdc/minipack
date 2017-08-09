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
            var packageDesc = PackageDescription.Read(sourcePath);

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
                outputDir = packageDesc.Name + "_" + version + "-" + revision;
            }

            string sourceDir = options.GetOption<string>("source-dir", null);
            if (string.IsNullOrWhiteSpace(sourceDir))
            {
                sourceDir = Environment.CurrentDirectory;
            }

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

            var partialControl = File.ReadAllText(Path.Combine(sourceDir, partialControlPath));

            PopulateTarget(
                log,
                packageDesc,
                partialControl,
                version,
                revision,
                sourceDir,
                outputDir);
        }

        private static void PopulateTarget(
            ICompilerLog log,
            PackageDescription package,
            string partialControl,
            string version,
            string revision,
            string sourceDirectory,
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
            var usrDirectory = Path.Combine(targetDirectory, "usr");
            package.CopyFilesToTarget(sourceDirectory, usrDirectory);

            // Instantiate executables.
            foreach (var exe in package.Executables)
            {
                InstantiateExecutable(log, exe, sourceDirectory, usrDirectory);
            }
        }

        private static readonly Dictionary<string, Action<ICompilerLog, ExecutableSpec, string, string>> executableHandlers =
            new Dictionary<string, Action<ICompilerLog, ExecutableSpec, string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "mono", InstantiateMonoExecutable }
        };

        private static void InstantiateExecutable(
            ICompilerLog log, ExecutableSpec spec, string sourceDirectory, string usrDirectory)
        {
            if (executableHandlers.ContainsKey(spec.Environment))
            {
                executableHandlers[spec.Environment](log, spec, sourceDirectory, usrDirectory);
            }
            else
            {
                log.LogError(
                    new LogEntry(
                        "unknown executable environment",
                        string.Format(
                            "deb-source does not understand executables that use the '{0}' environment.",
                            spec.Environment)));
            }
        }

        private static void InstantiateMonoExecutable(
            ICompilerLog log, ExecutableSpec spec, string sourceDirectory, string usrDirectory)
        {
            // We can run safely mono executables by generating a wrapper script. Such
            // a script looks like this:
            //
            //     #!/bin/sh
            //     exec /usr/bin/mono $MONO_OPTIONS /usr/file-path "$@"
            //

            var script = new StringBuilder();
            script.Append("#!/bin/sh\n");
            script.Append("exec /usr/bin/mono $MONO_OPTIONS /");
            script.Append(Path.Combine("usr", spec.File));
            script.Append(" \"$@\"\n");

            string scriptDir = Path.Combine(usrDirectory, "bin");
            string scriptPath = Path.Combine(scriptDir, spec.Name);
            Directory.CreateDirectory(scriptDir);
            File.WriteAllText(scriptPath, script.ToString());

            // Make our script executable.
            MakeExecutable(scriptPath);
        }

        private static void MakeExecutable(string path)
        {
            var processDesc = new ProcessStartInfo("chmod", "+x " + Path.GetFullPath(path));
            processDesc.RedirectStandardError = true;
            processDesc.UseShellExecute = false;
            var process = Process.Start(processDesc);
            process.WaitForExit();
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

