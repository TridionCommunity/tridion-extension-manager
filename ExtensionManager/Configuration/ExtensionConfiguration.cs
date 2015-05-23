using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using TridionCommunity.Extensions.Properties;
using TridionCommunity.Extensions.Configuration;
using System.Xml;

namespace TridionCommunity.Extensions
{
    /// <summary>
    /// Represents the metadata configuration for an extension and allows creating new instances based on it.
    /// </summary>
    internal class ExtensionConfiguration
    {
        protected string filePath;
        protected XDocument configuration;
        
        /// <summary>
        /// Loads the extension configuration from the ZIP file in the repository.
        /// </summary>
        /// <param name="zipFile">The name of the file to load.</param>
        /// <returns>An <see cref="ExtensionConfiguration"/> instance based on the configuration file.</returns>
        public static ExtensionConfiguration Load(string zipFile)
        {
            return new ExtensionConfiguration(zipFile);
        }

        /// <summary>
        /// Returns the full XML document containing the extension metadata.
        /// </summary>
        public XDocument Xml
        {
            get
            {
                return configuration;
            }
        }

        /// <summary>
        /// Creates a new instance of an <see cref="Extension"/> based on the configuration.
        /// </summary>
        /// <returns>The new <see cref="Extension"/> with values initialized from its configuration.</returns>
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

        /// <summary>
        /// Creates a new <see cref="ExtensionConfiguration"/> by reading the supplied ZIP file.
        /// </summary>
        /// <param name="zipFile">The path to the extension ZIP file in the repository.</param>
        /// <exception cref="ConfigurationException">If the ZIP file does not contain a valid <code>Extension.xml</code> file.</exception>
        protected ExtensionConfiguration(string zipFile)
        {
            this.filePath = zipFile;

            string extensionXml = ReadEntryFromZip(zipFile, @"Extension.xml");
            if (string.IsNullOrEmpty(extensionXml))
            {
                throw new ConfigurationException(Resources.ErrInvalidExtensionXml);
            }

            try
            {
                configuration = XDocument.Parse(extensionXml);
            }
            catch (XmlException ex)
            {
                throw new ConfigurationException(Resources.ErrInvalidExtensionXml, ex);
            }
        }

        /// <summary>
        /// Reads a specified file located inside of a ZIP archive.
        /// </summary>
        /// <param name="zipFile">The path to the ZIP archive containing the file.</param>
        /// <param name="entryName">The name of the file to read.</param>
        /// <returns>The entire content of the specified file.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
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

        /// <summary>
        /// Reads the string content of an XElement. Has silent error handling.
        /// </summary>
        /// <param name="element">The element whose text content you wish to read.</param>
        /// <returns>The text content of the element, if it exists. Returns null otherwise.</returns>
        private static string GetString(XElement element)
        {
            if (element != null)
            {
                return element.Value;
            }

            return null;
        }

        /// <summary>
        /// Reads the Version content of an XElement. Has silent error handling.
        /// </summary>
        /// <param name="element">The element whose content you wish to read.</param>
        /// <returns>The Version parsed from the text content of the element, if it exists and can be parsed as a Version. Returns null otherwise.</returns>
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

        /// <summary>
        /// Parses a full Editor or Model section from the extension configuration. Has silent error handling.
        /// </summary>
        /// <param name="element">The parent element (Editor or Model)</param>
        /// <returns>The section if it could be parsed. Null otherwise.</returns>
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

        /// <summary>
        /// Parses a list of Assembly elements from the configuration.
        /// </summary>
        /// <param name="container">The parent element (i.e. WebsiteAssemblies)</param>
        /// <returns>A list of strings containing the assembly paths.</returns>
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
