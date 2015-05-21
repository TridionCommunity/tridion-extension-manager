using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using TridionCommunity.Extensions.Properties;

namespace TridionCommunity.Extensions
{
    internal class Extension
    {
        public ExtensionInfo Info { get; set; }
        public SystemConfigSection Editor { get; set; }
        public SystemConfigSection Model { get; set; }
        public List<string> WebsiteAssemblies { get; set; }

        public string RepositoryFileName { get; set; }
        public string InstallPath { get; set; }
        public string SystemConfigFile { get; set; }


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

        internal void Disable()
        {
            AssertInitialized();

            var systemConfig = SystemConfiguration.Load(SystemConfigFile);
            systemConfig.RemoveEditor(Editor);
            systemConfig.RemoveModel(Model);
            systemConfig.Save();

            Info.Enabled = false;
        }

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
