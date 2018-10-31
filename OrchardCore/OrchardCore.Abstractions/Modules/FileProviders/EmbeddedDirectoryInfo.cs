using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules.FileProviders
{
    /// <summary>
    /// Represents a directory on a physical filesystem
    /// </summary>
    public class EmbeddedDirectoryInfo : IFileInfo
    {
        private string _name;

        /// <summary>
        /// Initializes an instance of <see cref="EmbeddedDirectoryInfo"/>
        /// </summary>
        /// <param name="info">The directory</param>
        public EmbeddedDirectoryInfo(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Always true.
        /// </summary>
        public bool Exists => true;

        /// <summary>
        /// Always equals -1.
        /// </summary>
        public long Length => -1;

        /// <summary>
        /// Always null.
        /// </summary>
        public string PhysicalPath => null;

        /// <inheritdoc />
        public string Name => _name;

        /// <summary>
        /// The time when the directory was last written to.
        /// </summary>
        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        /// <summary>
        /// Always true.
        /// </summary>
        public bool IsDirectory => true;

        /// <summary>
        /// Always throws an exception because read streams are not support on directories.
        /// </summary>
        /// <exception cref="InvalidOperationException">Always thrown</exception>
        /// <returns>Never returns</returns>
        public Stream CreateReadStream()
        {
            throw new InvalidOperationException("Cannot create a stream for a directory.");
        }
    }
}