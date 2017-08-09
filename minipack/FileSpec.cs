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
    }
}