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
    /// A subprogram that copies files to a directory that is organized
    /// like a Unix OS' file system
    /// </summary>
    public sealed class UnixInstallSubprogram : Subprogram
    {
        private UnixInstallSubprogram()
        { }

        /// <summary>
        /// An instance of the unix-install subprogram.
        /// </summary>
        public static readonly UnixInstallSubprogram Instance = new UnixInstallSubprogram();

        /// <inheritdoc/>
        public override string Description => "Organizes files like a Unix OS' file system.";

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
                FilesSubprogram.LogInvalidSourcePathArity(log, options.SourcePaths.Length);
                return;
            }

            string sourcePath = options.SourcePaths[0].ToString();
            var packageDesc = PackageDescription.Read(sourcePath);

            string outputDir = options.GetOption<string>("o", null);
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = "/usr/local/";
            }

            string sourceDir = options.GetOption<string>("source-dir", null);
            if (string.IsNullOrWhiteSpace(sourceDir))
            {
                sourceDir = Environment.CurrentDirectory;
            }

            Install(
                log,
                packageDesc,
                sourceDir,
                outputDir);
        }

        /// <summary>
        /// Organizes the given package's files in a Unix file system.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="package">A description of the package to install.</param>
        /// <param name="sourceDirectory">A path to the directory that contains the package's source files.</param>
        /// <param name="targetDirectory">A path to the directory that contains the package's target files.</param>
        public static void Install(
            ICompilerLog log,
            PackageDescription package,
            string sourceDirectory,
            string targetDirectory)
        {
            var namedPaths = new Dictionary<string, string>();
            namedPaths[TargetDescription.ExecutableDirectoryPathName] = Path.Combine("lib", package.Name);

            var targetDesc = new TargetDescription(targetDirectory, namedPaths);

            package.CopyFilesToTarget(sourceDirectory, targetDesc, AfterCopyFile);

            // Instantiate executables.
            foreach (var exe in package.Executables)
            {
                InstantiateExecutable(log, exe, sourceDirectory, targetDesc);
            }
        }

        private static readonly Dictionary<string, Action<ICompilerLog, ExecutableSpec, string, TargetDescription>> executableHandlers =
            new Dictionary<string, Action<ICompilerLog, ExecutableSpec, string, TargetDescription>>(StringComparer.OrdinalIgnoreCase)
        {
            { "mono", InstantiateMonoExecutable }
        };

        private static void InstantiateExecutable(
            ICompilerLog log, ExecutableSpec spec, string sourceDirectory, TargetDescription targetDesc)
        {
            if (executableHandlers.ContainsKey(spec.Environment))
            {
                executableHandlers[spec.Environment](log, spec, sourceDirectory, targetDesc);
            }
            else
            {
                log.LogError(
                    new LogEntry(
                        "unknown executable environment",
                        string.Format(
                            "subprogram does not understand executables that use the '{0}' environment.",
                            spec.Environment)));
            }
        }

        private static void InstantiateMonoExecutable(
            ICompilerLog log, ExecutableSpec spec, string sourceDirectory, TargetDescription targetDesc)
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
            script.Append(Path.Combine("usr", targetDesc.ExpandTargetPathRelative(spec.File)));
            script.Append(" \"$@\"\n");

            string scriptDir = Path.Combine(targetDesc.RootTargetDirectory, "bin");
            string scriptPath = Path.Combine(scriptDir, spec.Name);
            Directory.CreateDirectory(scriptDir);
            File.WriteAllText(scriptPath, script.ToString());

            // Make our script executable.
            MakeExecutable(scriptPath);
        }

        private static void AfterCopyFile(string sourcePath, string targetPath)
        {
            if (Path.GetExtension(targetPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                MakeExecutable(targetPath);
            }
            else
            {
                MakeNonExecutable(targetPath);
            }
        }

        private static void MakeExecutable(string path)
        {
            Chmod(path, "+x");
        }

        private static void MakeNonExecutable(string path)
        {
            Chmod(path, "-x");
        }

        private static void Chmod(string path, string options)
        {
            var processDesc = new ProcessStartInfo("chmod", options + " " + Path.GetFullPath(path));
            processDesc.UseShellExecute = false;
            var process = Process.Start(processDesc);
            process.WaitForExit();
        }
    }
}

