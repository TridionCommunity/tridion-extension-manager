
namespace TridionCommunity.Extensions
{
    /// <summary>
    /// Contains the information needed to set up an editor or model in the CME.
    /// </summary>
    internal class SystemConfigSection
    {
        /// <summary>The name of the editor or model.</summary>
        public string Name { get; set; }
        /// <summary>The relative path to the source of the editor/model.</summary>
        public string Path { get; set; }
        /// <summary>The relative path to the configuration file for the editor/model.</summary>
        public string ConfigurationFile { get; set; }
        /// <summary>The name of the virtual directory for the editor/model.</summary>
        public string VirtualDirectory { get; set; }
    }
}
