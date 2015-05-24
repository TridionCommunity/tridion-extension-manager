using System.Collections.Generic;
using System.IO;

namespace TridionCommunity.Extensions
{
    /// <summary>
    /// Responsible for actions related to the Tridion Content Manager website.
    /// </summary>
    internal static class Website
    {
        /// <summary>
        /// Adds a new virtual directory as a child of the WebUI virtual directory.
        /// </summary>
        /// <param name="name">The name of the new virtual directory</param>
        public static void AddVirtualDirectory(string name)
        {
        }

        /// <summary>
        /// Removes the given virtual directory from the parent WebUI virtual directory.
        /// </summary>
        /// <param name="name">The name of the virtual directory to remove.</param>
        public static void RemoveVirtualDirectory(string name)
        {
        }

        /// <summary>
        /// Copies the specified assemblies to the WebUI\WebRoot\bin folder so they can be called from the CME.
        /// </summary>
        /// <param name="assemblies">A list of the files to copy.</param>
        public static void AddAssembliesToBin(List<FileInfo> assemblies)
        {
        }

        /// <summary>
        /// Removes the specified assemblies from the WebUI\WebRoot\bin folder. Used when uninstalling an extension.
        /// </summary>
        /// <param name="assemblyNames">The list of filenames of the assemblies to remove.</param>
        public static void RemoveAssembliesFromBin(List<string> assemblyNames)
        {
        }
    }
}
