using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Minipack
{
    /// <summary>
    /// A high-level description of a package.
    /// </summary>
    public sealed class PackageDescription
    {
        [JsonConstructor]
        public PackageDescription()
        {
            this.Name = "";
            this.TargetConfigs = new Dictionary<string, Dictionary<string, string>>();
            this.Files = new List<FileSpec>();
            this.Executables = new List<ExecutableSpec>();
        }

        /// <summary>
        /// Creates a new package description from the given name, target configurations,
        /// files and executables.
        /// </summary>
        /// <param name="Name">The name of the package.</param>
        /// <param name="TargetConfigs">The package's configuration per target.</param>
        /// <param name="Files">The files in the package.</param>
        /// <param name="Executables">The executables in the package.</param>
        public PackageDescription(
            string Name,
            Dictionary<string, Dictionary<string, string>> TargetConfigs,
            List<FileSpec> Files,
            List<ExecutableSpec> Executables)
        {
            this.Name = Name;
            this.TargetConfigs = TargetConfigs;
            this.Files = Files;
            this.Executables = Executables;
        }

        /// <summary>
        /// Gets the name of the package.
        /// </summary>
        /// <returns>The name of the package.</returns>
        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// Gets the package's configuration per target.
        /// </summary>
        /// <returns>The package's configuration per target.</returns>
        [JsonProperty("targets")]
        [JsonRequired]
        public Dictionary<string, Dictionary<string, string>> TargetConfigs { get; set; }

        /// <summary>
        /// Gets a read-only list of files listed in the package.
        /// </summary>
        /// <returns>The files in the package.</returns>
        [JsonProperty("files")]
        [JsonRequired]
        public List<FileSpec> Files { get; set; }

        /// <summary>
        /// Gets a read-only list of executables specified by the package.
        /// </summary>
        /// <returns>The executables in the package.</returns>
        [JsonProperty("executables")]
        [JsonRequired]
        public List<ExecutableSpec> Executables { get; set; }

        /// <summary>
        /// Gets the target configuration with the given name or <c>null</c> if that configuration
        /// doesn't exist.
        /// </summary>
        /// <param name="targetName">The name of the target configuration to find.</param>
        /// <returns>The target configuration with the given name.</returns>
        public Dictionary<string, string> GetConfigOrNull(string targetName)
        {
            if (TargetConfigs.ContainsKey(targetName))
                return TargetConfigs[targetName];
            else
                return null;
        }

        /// <summary>
        /// Copies the files in this package description from the source
        /// directory to the target directory.
        /// </summary>
        /// <param name="sourceDirectory">A path to the source directory.</param>
        /// <param name="target">A description of the target directory.</param>
        /// <param name="afterCopy">
        /// A callback that is run on the (source path, target path) pair of each copied file.
        /// </param>
        public void CopyFilesToTarget(
            string sourceDirectory,
            TargetDescription target,
            Action<string, string> afterCopy)
        {
            foreach (var file in Files)
            {
                file.CopyToTarget(sourceDirectory, target, afterCopy);
            }
        }

        /// <summary>
        /// Copies the files in this package description from the source
        /// directory to the target directory.
        /// </summary>
        /// <param name="sourceDirectory">A path to the source directory.</param>
        /// <param name="target">A description of the target directory.</param>
        public void CopyFilesToTarget(
            string sourceDirectory,
            TargetDescription target)
        {
            foreach (var file in Files)
            {
                file.CopyToTarget(sourceDirectory, target);
            }
        }

        /// <summary>
        /// Reads the package description at the given file.
        /// </summary>
        /// <param name="path">A path to the file to read the package description from.</param>
        /// <returns>A package description.</returns>
        public static PackageDescription Read(string path)
        {
            PackageDescription result;
            using (var file = File.OpenText(path))
            {
                result = JsonConvert.DeserializeObject<PackageDescription>(file.ReadToEnd());
            }
            return result;
        }
    }
}