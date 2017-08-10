using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Minipack
{
    /// <summary>
    /// Describes a target directory.
    /// </summary>
    public sealed class TargetDescription
    {
        /// <summary>
        /// Creates a target description from the given root target directory
        /// path and a read-only dictionary of named paths.
        /// </summary>
        /// <param name="RootTargetDirectory">A path to the root target directory.</param>
        /// <param name="NamedPaths">A dictionary of named paths.</param>
        public TargetDescription(
            string RootTargetDirectory,
            IReadOnlyDictionary<string, string> NamedPaths)
        {
            this.RootTargetDirectory = RootTargetDirectory;
            this.NamedPaths = NamedPaths;
        }

        /// <summary>
        /// Gets a path to the root target directory.
        /// </summary>
        /// <returns>A path to the root target directory.</returns>
        public string RootTargetDirectory { get; private set; }

        /// <summary>
        /// Gets the named paths in the target.
        /// </summary>
        /// <returns>A dictionary of named paths.</returns>
        public IReadOnlyDictionary<string, string> NamedPaths { get; private set; }

        /// <summary>
        /// Gets the named path with the given name if it exists.
        /// </summary>
        /// <param name="name">The name of the named path.</param>
        /// <param name="path">The named path.</param>
        /// <returns>
        /// <c>true</c> if a named path with the given name exists; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetNamedPath(string name, out string path)
        {
            return NamedPaths.TryGetValue(name, out path);
        }

        /// <summary>
        /// Expands a path relative to the target directory.
        /// </summary>
        /// <param name="relativePath">The path to expand.</param>
        /// <returns>The expanded path.</returns>
        public string ExpandTargetPath(string relativePath)
        {
            var expandedPath = new StringBuilder();
            int spanStart = 0;
            for (int i = 0; i < relativePath.Length; i++)
            {
                if (i >= 1 && relativePath.Substring(i - 1, 2) == "${")
                {
                    expandedPath.Append(relativePath, spanStart, i - spanStart - 1);
                    int closingIndex = relativePath.IndexOf('}', i);
                    if (closingIndex <= i)
                    {
                        throw new TargetPathExpansionException(
                            "'${' does not have a closing '}' in path '" +
                            relativePath + "'.");
                    }
                    string pathName = relativePath.Substring(i + 1, closingIndex - i - 1);
                    string namedPath;
                    if (!TryGetNamedPath(pathName, out namedPath))
                    {
                        throw new TargetPathExpansionException(
                            "named path '${" + pathName + "}' does not exist in this target.");
                    }
                    i = closingIndex;
                    spanStart = i + 1;

                    expandedPath.Append(namedPath);
                }
            }
            expandedPath.Append(relativePath, spanStart, relativePath.Length - spanStart);
            return Path.Combine(RootTargetDirectory, expandedPath.ToString());
        }

        /// <summary>
        /// Gets the name of the executable directory named path.
        /// </summary>
        public const string ExecutableDirectoryPathName = "exe";
    }

    /// <summary>
    /// An exception that is thrown when a target path cannot be expanded for some reason.
    /// </summary>
    [Serializable]
    public class TargetPathExpansionException : Exception
    {
        public TargetPathExpansionException(string message) : base(message) { }
        public TargetPathExpansionException(string message, Exception inner) : base(message, inner) { }
        protected TargetPathExpansionException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}