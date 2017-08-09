using Newtonsoft.Json;

namespace Minipack
{
    /// <summary>
    /// Describes an executable in a package.
    /// </summary>
    public sealed class ExecutableSpec
    {
        [JsonConstructor]
        public ExecutableSpec()
        {

        }

        /// <summary>
        /// Creates an executable spec from the given name, environment and file.
        /// </summary>
        /// <param name="Name">The name of the executable.</param>
        /// <param name="Environment">The execution environment for the executable.</param>
        /// <param name="File">A path to the executable file.</param>
        public ExecutableSpec(string Name, string Environment, string File)
        {
            this.Name = Name;
            this.Environment = Environment;
            this.File = File;
        }

        /// <summary>
        /// Gets the name of the executable.
        /// </summary>
        /// <returns>The name of the executable.</returns>
        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// Gets the name of the execution environment for the executable.
        /// </summary>
        /// <returns>The name of the execution environment for the executable.</returns>
        [JsonProperty("environment")]
        [JsonRequired]
        public string Environment { get; set; }

        /// <summary>
        /// Gets a path to the executable file.
        /// </summary>
        /// <returns>A path to the executable.</returns>
        [JsonProperty("file")]
        [JsonRequired]
        public string File { get; set; }
    }
}