using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Minipack
{
    /// <summary>
    /// Describes zero or more files in a package.
    /// </summary>
    public sealed class FileSpec
    {
        [JsonConstructor]
        public FileSpec()
        {

        }

        /// <summary>
        /// Creates a file spec from the given source and target path.
        /// </summary>
        /// <param name="Source">The source path.</param>
        /// <param name="Target">The target path.</param>
        public FileSpec(string Source, string Target)
        {
            this.Source = Source;
            this.Target = Target;
        }

        /// <summary>
        /// Gets a path to the files which are copied.
        /// </summary>
        /// <returns>The source path.</returns>
        [JsonProperty("src")]
        [JsonRequired]
        public string Source { get; set; }

        /// <summary>
        /// Gets a path to the file or directory to which the file is copied.
        /// </summary>
        /// <returns>The target path.</returns>
        [JsonProperty("target")]
        [JsonRequired]
        public string Target { get; set; }

        /// <summary>
        /// Tells if this file spec's target represents a directory.
        /// </summary>
        /// <returns><c>true</c> if the target is a directory; otherwise, <c>false</c>.</returns>
        /// <remarks>All targets that end in '/' are considered directories.</remarks>
        public bool TargetIsDirectory => Target.EndsWith("/");

        /// <summary>
        /// Gets all source files specified by this file spec.
        /// </summary>
        /// <returns>The source files.</returns>
        public IReadOnlyList<string> GetSourceFiles()
        {
            return Directory.GetFiles(Environment.CurrentDirectory, Source);
        }

        /// <summary>
        /// Creates a mapping of source files to their respective target files.
        /// </summary>
        /// <returns>A mapping of source files to their respective target files.</returns>
        public IReadOnlyDictionary<string, string> GetTargetMapping()
        {
            return GetTargetMapping(GetSourceFiles());
        }

        /// <summary>
        /// Creates a mapping of source files to their respective target files.
        /// </summary>
        /// <param name="SourceFiles">The source files to map to target files.</param>
        /// <returns>A mapping of source files to their respective target files.</returns>
        public IReadOnlyDictionary<string, string> GetTargetMapping(IEnumerable<string> SourceFiles)
        {
            var results = new Dictionary<string, string>();
            if (TargetIsDirectory)
            {
                foreach (var item in SourceFiles)
                {
                    results[item] = Path.Combine(Target, item);
                }
            }
            else
            {
                foreach (var item in SourceFiles)
                {
                    results[item] = Target;
                }
            }
            return results;
        }
    }
}