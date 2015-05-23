using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TridionCommunity.Extensions.Properties;

namespace TridionCommunity.Extensions
{
    /// <summary>
    /// Manages available extensions, allowing you to install, uninstall, enable, or disable them.
    /// </summary>
    public class ExtensionManager
    {
        /// <summary>The path to the directory containing all managed extensions in their compressed form.</summary>
        public string RepositoryLocation { get; protected set; }
        /// <summary>The path to the directory where the extensions are installed.</summary>
        public string ExtensionsDirectory { get; protected set; }
        /// <summary>The path to the System.config file of the Content Manager Explorer.</summary>
        public string SystemConfigFile { get; protected set; }


        private List<Extension> extensions = new List<Extension>();


        /// <summary>
        /// Creates a new instance of the ExtensionManager class.
        /// </summary>
        /// <param name="repositoryLocation">The path to the repository of available extensions.</param>
        /// <param name="extensionsDirectory">The directory where the extensions should be installed.</param>
        /// <param name="systemConfigPath">The path to the System.config configuration file of the Content Manager Explorer.</param>
        /// <exception cref="System.ArgumentException">If any of the directories or files specified in the arguments do not exist.</exception>
        public ExtensionManager(string repositoryLocation, string extensionsDirectory, string systemConfigPath)
        {
            if (!Directory.Exists(repositoryLocation))
            {
                throw new ArgumentException(Resources.ErrDirectoryDoesNotExist, @"repositoryLocation");
            }

            if (!Directory.Exists(extensionsDirectory))
            {
                throw new ArgumentException(Resources.ErrDirectoryDoesNotExist, @"extensionsDirectory");
            }

            if (!File.Exists(systemConfigPath))
            {
                throw new ArgumentException(Resources.ErrFileDoesNotExist, @"systemConfigPath");
            }

            RepositoryLocation = repositoryLocation;
            ExtensionsDirectory = extensionsDirectory;
            SystemConfigFile = systemConfigPath;
        }

        /// <summary>
        /// Get a list of all available extensions and their status.
        /// </summary>
        /// <returns>The list of available extensions and their status, along with metadata needed to manage them.</returns>
        public List<ExtensionInfo> GetExtensions()
        {
            extensions = new List<Extension>();

            var files = Directory.GetFiles(RepositoryLocation, @"*.zip");
            foreach (var file in files)
            {
                var extension = LoadExtension(file);
                if (extension != null)
                {
                    extensions.Add(extension);
                }
            }

            var result = new List<ExtensionInfo>();
            result.AddRange(extensions.Select(e => e.Info));
            result.Sort();
            return result;
        }

        /// <summary>
        /// Install the given extension in a disabled state.
        /// </summary>
        /// <param name="extensionInfo">The extension to install.</param>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="extensionInfo"/> argument is null.</exception>
        public void Install(ExtensionInfo extensionInfo)
        {
            if (extensionInfo == null)
            {
                throw new ArgumentNullException(@"extensionInfo");
            }

            var extension = FindExtension(extensionInfo);
            extension.Install();
            extensionInfo.UpdateFrom(extension.Info);
        }

        /// <summary>
        /// Uninstalls the given extension. It will still be available in the repository for later.
        /// </summary>
        /// <param name="extensionInfo">The extension to uninstall.</param>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="extensionInfo"/> argument is null.</exception>
        public void Uninstall(ExtensionInfo extensionInfo)
        {
            if (extensionInfo == null)
            {
                throw new ArgumentNullException(@"extensionInfo");
            }

            var extension = FindExtension(extensionInfo);
            extension.Uninstall();
            extensionInfo.UpdateFrom(extension.Info);
        }

        /// <summary>
        /// Enables the given extension, making it available for use immediately. 
        /// </summary>
        /// <param name="extensionInfo">The extension to enable.</param>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="extensionInfo"/> argument is null.</exception>
        public void Enable(ExtensionInfo extensionInfo)
        {
            if (extensionInfo == null)
            {
                throw new ArgumentNullException(@"extensionInfo");
            }

            var extension = FindExtension(extensionInfo);
            extension.Enable();
            extensionInfo.UpdateFrom(extension.Info);
        }

        /// <summary>
        /// Disables the given extension. Takes effect immediately.
        /// </summary>
        /// <param name="extensionInfo">The extension to disable.</param>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="extensionInfo"/> argument is null.</exception>
        public void Disable(ExtensionInfo extensionInfo)
        {
            if (extensionInfo == null)
            {
                throw new ArgumentNullException(@"extensionInfo");
            }
            
            var extension = FindExtension(extensionInfo);
            extension.Disable();
            extensionInfo.UpdateFrom(extension.Info);
        }

        /// <summary>
        /// Loads an extension from its reposity file and updates its status based on the current state of the system.
        /// </summary>
        /// <param name="zipFile">The ZIP file to load the extension from.</param>
        /// <returns>A fully loaded extension with an up-to-date state.</returns>
        private Extension LoadExtension(string zipFile)
        {
            var configuration = ExtensionConfiguration.Load(zipFile);
            if (configuration != null)
            {
                var result = configuration.CreateInstance();
                result.InstallPath = Path.Combine(ExtensionsDirectory, result.Info.Name);
                result.RepositoryFileName = zipFile;
                result.SystemConfigFile = SystemConfigFile;
                result.RefreshStatus();
                return result;
            }
            return null;
        }

        /// <summary>
        /// Looks up the extension instance based on the info provided.
        /// </summary>
        /// <param name="extensionInfo">The extension info presented to the outside world.</param>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="extensionInfo"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">If the <paramref name="extensionInfo"/> does not relate to a known extension.</exception>
        /// <returns>The extension instance that corresponds to the info provided.</returns>
        private Extension FindExtension(ExtensionInfo extensionInfo)
        {
            if (extensionInfo == null)
            {
                throw new ArgumentNullException(@"extensionInfo");
            }

            var result = extensions.FirstOrDefault(e => e.Info.Name == extensionInfo.Name);
            if (result == null)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, Resources.ErrInvalidExtensionInfo, extensionInfo.Name), @"extensionInfo");
            }
            return result;
        }
    }
}
