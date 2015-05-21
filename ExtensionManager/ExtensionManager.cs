using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TridionCommunity.Extensions.Properties;

namespace TridionCommunity.Extensions
{
    public class ExtensionManager
    {
        public string RepositoryLocation { get; protected set; }
        public string ExtensionsDirectory { get; protected set; }
        public string SystemConfigFile { get; protected set; }

        private List<Extension> extensions = new List<Extension>();


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

        private Extension FindExtension(ExtensionInfo extensionInfo)
        {
            if (extensionInfo == null)
            {
                throw new ArgumentNullException(@"extensionInfo");
            }

            var result = extensions.FirstOrDefault(e => e.Info.Name == extensionInfo.Name);
            if (result == null)
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, Resources.ErrInvalidExtensionInfo, extensionInfo.Name));
            }
            return result;
        }
    }
}
