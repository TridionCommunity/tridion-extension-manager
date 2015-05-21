using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using TridionCommunity.Extensions.Properties;

namespace TridionCommunity.Extensions
{
    internal class ExtensionConfiguration
    {
        protected string filePath;
        protected XDocument configuration;
        
        public static ExtensionConfiguration Load(string zipFile)
        {
            return new ExtensionConfiguration(zipFile);
        }

        public XDocument Xml
        {
            get
            {
                return configuration;
            }
        }

        internal Extension CreateInstance()
        {
            if (configuration == null)
            {
                return null;
            }

            var root = configuration.Element(@"Extension");
            if (root == null) return null;

            return new Extension
            {
                Info = new ExtensionInfo
                {
                    Name = GetString(root.Element(@"Name")),
                    Description = GetString(root.Element(@"Description")),
                    Icon = GetString(root.Element(@"Icon")),
                    Author = GetString(root.Element(@"Author")),
                    Header = GetString(root.Element(@"Header")),
                    Version = GetVersion(root.Element(@"Version"))
                },
                Editor = GetConfigurationInfo(root.Element(@"Editor")),
                Model = GetConfigurationInfo(root.Element(@"Model")),
                WebsiteAssemblies = GetAssemblyList(root.Element(@"WebsiteAssemblies"))
            };
        }


        protected ExtensionConfiguration(string zipFile)
        {
            this.filePath = zipFile;

            string extensionXml = ReadEntryFromZip(zipFile, @"Extension.xml");
            if (string.IsNullOrEmpty(extensionXml))
            {
                throw new Exception(Resources.ErrInvalidExtensionXml);
            }

            configuration = XDocument.Parse(extensionXml);
        }

        protected string ReadEntryFromZip(string zipFile, string entryName)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (ZipFile file = ZipFile.Read(zipFile))
                    {
                        var manifest = file[entryName];
                        using (var stream = manifest.OpenReader())
                        {
                            var buffer = new byte[2048];
                            int n;
                            while ((n = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, n);
                            }
                        }
                    }

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(ms))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static string GetString(XElement element)
        {
            if (element != null)
            {
                return element.Value;
            }

            return null;
        }

        private static Version GetVersion(XElement element)
        {
            if (element != null)
            {
                Version result;
                if (Version.TryParse(element.Value, out result))
                {
                    return result;
                }
            }

            return new Version(0, 0);
        }

        private static SystemConfigSection GetConfigurationInfo(XElement element)
        {
            if (element != null)
            {
                return new SystemConfigSection
                {
                    Name = GetString(element.Element(@"Name")),
                    Path = GetString(element.Element(@"Path")),
                    ConfigurationFile = GetString(element.Element(@"Configuration")),
                    VirtualDirectory = GetString(element.Element(@"VirtualDirectory"))
                };
            }
            return null;
        }

        private static List<string> GetAssemblyList(XElement container)
        {
            var result = new List<string>();
            if (container == null)
            {
                return result;
            }

            var elements = container.Elements(@"Assembly");
            if (elements != null)
            {
                result.AddRange(elements.Select(e => e.Value));
            }
            return result;
        }
    }
}
