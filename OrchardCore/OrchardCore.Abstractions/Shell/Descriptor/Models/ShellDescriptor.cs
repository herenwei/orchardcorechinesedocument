using System.Collections.Generic;

namespace OrchardCore.Environment.Shell.Descriptor.Models
{
    /// <summary>
    /// Contains a snapshot of a tenant's enabled features.
    /// The information is drawn out of the shell via IShellDescriptorManager
    /// and is passed to the ICompositionStrategy to build the ShellBlueprint.
    /// </summary>
    public class ShellDescriptor
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the version number of the shell descriptor.
        /// </summary>
        public int SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of features in the shell.
        /// </summary>
        public IList<ShellFeature> Features { get; set; } = new List<ShellFeature>();

        /// <summary>
        /// Gets or sets the list of parameters specific to this shell.
        /// </summary>
        public IList<ShellParameter> Parameters { get; set; } = new List<ShellParameter>();
    }
}