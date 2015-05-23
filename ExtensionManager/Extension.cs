using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using TridionCommunity.Extensions.Properties;

namespace TridionCommunity.Extensions
{
    /// <summary>
    /// Contains all of the information available about an extension as well as the operations you can do on it (install, uninstall, disable, etc.)
    /// </summary>
    internal class Extension
    {
        /// <summary>The metadata information about the extension. See <see cref="ExtensionInfo"/> for details.</summary>
        public ExtensionInfo Info { get; set; }
        /// <summary>The configuration of the extension's editor.</summary>
        public SystemConfigSection Editor { get; set; }
        /// <summary>The configuration of the extension's model.</summary>
        public SystemConfigSection Model { get; set; }
        /// <summary>The list of assemblies to install into the <code>bin</code> folder of the CME website.</summary>
        public List<string> WebsiteAssemblies { get; set; }

        /// <summary>The name of the extension ZIP file within the repository.</summary>
        public string RepositoryFileName { get; set; }
        /// <summary>The directory that the extension should be installed in.</summary>
        public string InstallPath { get; set; }
        /// <summary>The path to the System.config file from the CME.</summary>
        public string SystemConfigFile { get; set; }


        /// <summary>
        /// Installs the extension in a disabled state.
        /// </summary>
        internal void Install()
        {
            AssertInitialized();

            using (var file = ZipFile.Read(RepositoryFileName))
            {
                file.ExtractAll(InstallPath, ExtractExistingFileAction.OverwriteSilently);

                if (Editor != null && !string.IsNullOrWhiteSpace(Editor.VirtualDirectory))
                {
                    Website.AddVirtualDirectory(Editor.VirtualDirectory);
                }

                if (Model != null && !string.IsNullOrWhiteSpace(Model.VirtualDirectory))
                {
                    Website.AddVirtualDirectory(Model.VirtualDirectory);
                }

                if (WebsiteAssemblies.Count > 0)
                {
                    var assemblies = new List<FileInfo>();
                    foreach (string filePath in WebsiteAssemblies)
                    {
                        string sourcePath = Path.Combine(InstallPath, filePath);
                        if (File.Exists(sourcePath))
                        {
                            assemblies.Add(new FileInfo(sourcePath));
                        }
                    }

                    Website.AddAssembliesToBin(assemblies);
                }

                Info.Status = InstallState.Installed;
            }
        }

        /// <summary>
        /// Disables and uninstalls the extension.
        /// </summary>
        internal void Uninstall()
        {
            AssertInitialized();

            if (Info.Enabled)
            {
                Disable();
            }

            if (Directory.Exists(InstallPath))
            {
                Directory.Delete(InstallPath, true);
            }

            if (Editor != null && !string.IsNullOrWhiteSpace(Editor.VirtualDirectory))
            {
                Website.RemoveVirtualDirectory(Editor.VirtualDirectory);
            }

            if (Model != null && !string.IsNullOrWhiteSpace(Model.VirtualDirectory))
            {
                Website.RemoveVirtualDirectory(Model.VirtualDirectory);
            }

            if (WebsiteAssemblies.Count > 0)
            {
                var assemblyNames = new List<string>();
                foreach (string filePath in WebsiteAssemblies)
                {
                    assemblyNames.Add(Path.GetFileName(filePath));
                }

                Website.RemoveAssembliesFromBin(assemblyNames);
            }


            Info.Status = InstallState.Uninstalled;
        }

        /// <summary>
        /// Enables the extension for immediate use.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">If the extension has not been installed.</exception>
        internal void Enable()
        {
            AssertInitialized();

            if (Info.Status != InstallState.Installed)
            {
                throw new InvalidOperationException(Resources.ErrCannotEnableUninstalledExtension);
            }

            var systemConfig = SystemConfiguration.Load(SystemConfigFile);
            systemConfig.AddEditor(Editor, InstallPath);
            systemConfig.AddModel(Model, InstallPath);
            systemConfig.Save();
            
            Info.Enabled = true;
        }

        /// <summary>
        /// Disables the extension. Takes effect immediately.
        /// </summary>
        internal void Disable()
        {
            AssertInitialized();

            var systemConfig = SystemConfiguration.Load(SystemConfigFile);
            systemConfig.RemoveEditor(Editor);
            systemConfig.RemoveModel(Model);
            systemConfig.Save();

            Info.Enabled = false;
        }

        /// <summary>
        /// Refreshes the Status and Enabled properties based on the current state of the system.
        /// </summary>
        internal void RefreshStatus()
        {
            Info.Status = InstallState.Uninstalled;
            if (Directory.Exists(InstallPath))
            {
                Info.Status = InstallState.Installed;
            }

            Info.Enabled = false;

            var systemConfig = SystemConfiguration.Load(SystemConfigFile);
            if (systemConfig.HasEditor(Editor) || systemConfig.HasModel(Model))
            {
                Info.Enabled = true;
            }
        }

        /// <summary>
        /// Validates that the properties required for the other methods have been set.
        /// </summary>
        private void AssertInitialized()
        {
            if (string.IsNullOrWhiteSpace(RepositoryFileName))
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, Resources.ErrRequiredPropertyNotSet, @"RepositoryFileName"));
            }

            if (string.IsNullOrWhiteSpace(InstallPath))
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, Resources.ErrRequiredPropertyNotSet, @"InstallPath"));
            }

            if (string.IsNullOrWhiteSpace(SystemConfigFile))
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, Resources.ErrRequiredPropertyNotSet, @"SystemConfigFile"));
            }
        }
    }
}
