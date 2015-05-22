using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TridionCommunity.Extensions;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tests
{
    [TestClass]
    public class ExtensionManagerTests
    {
        protected string repository = @"Repository";
        protected string installationDirectory = @"Extensions";
        protected string systemConfig = @"Configuration\System.config";

        protected XNamespace c = "http://www.sdltridion.com/2009/GUI/Configuration";

        protected const int numberOfExtensions = 1;

        [TestInitialize]
        public void Initialize()
        {
            Assert.IsTrue(Directory.Exists(repository), @"Repository has not been copied to the current directory.");

            // Clean up the installation directory so our tests have a clean slate
            if (Directory.Exists(installationDirectory))
            {
                Directory.Delete(installationDirectory, true);
            }

            Directory.CreateDirectory(installationDirectory);
        }

        [TestMethod]
        public void TestExtensionList()
        {
            var manager = new ExtensionManager(repository, installationDirectory, systemConfig);
            var extensions = manager.GetExtensions();
            Assert.AreEqual(numberOfExtensions, extensions.Count);

            var extension = extensions.FirstOrDefault(x => x.Name == "Extension Name");
            Assert.IsNotNull(extension, @"Failed to locate the test extension.");
            Assert.AreEqual(@"This is the description.</br>It can <i>even</i> contain <strong>HTML</strong> markup!", extension.Description);
            Assert.AreEqual(@"Peter Kjaer (SDL)", extension.Author);
            Assert.AreEqual(@"Icon.png", extension.Icon);
            Assert.AreEqual(@"Cool Tools", extension.Header);
            Assert.AreEqual(InstallState.Uninstalled, extension.Status);
            Assert.IsFalse(extension.Enabled);
            Assert.AreEqual(new Version(1, 2), extension.Version);
        }

        [TestMethod]
        public void TestInstallation()
        {
            var manager = new ExtensionManager(repository, installationDirectory, systemConfig);
            var extensions = manager.GetExtensions();
            Assert.AreEqual(numberOfExtensions, extensions.Count);

            var extension = extensions.FirstOrDefault(x => x.Name == "Extension Name");
            Assert.IsNotNull(extension, @"Failed to locate the test extension.");
            manager.Install(extension);

            Assert.AreEqual(InstallState.Installed, extension.Status);
            Assert.IsFalse(extension.Enabled);
            string extensionDirectory = Path.Combine(installationDirectory, extension.Name);
            Assert.IsTrue(Directory.Exists(extensionDirectory));
            Assert.IsTrue(File.Exists(Path.Combine(extensionDirectory, @"Editor", @"Scripts", @"Something.js")));
            Assert.IsTrue(File.Exists(Path.Combine(extensionDirectory, @"Model", @"Scripts", @"MyItem.js")));

            manager.Uninstall(extension);
            Assert.AreEqual(InstallState.Uninstalled, extension.Status);
            Assert.IsFalse(extension.Enabled);
            Assert.IsFalse(Directory.Exists(extensionDirectory));
        }

        [TestMethod]
        public void TestEnableDisable()
        {
            var manager = new ExtensionManager(repository, installationDirectory, systemConfig);
            var extensions = manager.GetExtensions();
            Assert.AreEqual(numberOfExtensions, extensions.Count);

            var extension = extensions.FirstOrDefault(x => x.Name == "Extension Name");
            Assert.IsNotNull(extension, @"Failed to locate the test extension.");
            manager.Install(extension);

            Assert.AreEqual(InstallState.Installed, extension.Status);
            Assert.IsFalse(extension.Enabled);

            // Enable the extension
            manager.Enable(extension);
            Assert.AreEqual(InstallState.Installed, extension.Status);
            Assert.IsTrue(extension.Enabled);

            var config = XDocument.Load(systemConfig);

            // Validate that System.config contains the changes
            var serverNode = config.Descendants(c + @"server").FirstOrDefault();
            Assert.IsNotNull(serverNode);
            Assert.AreEqual("1", serverNode.Attribute("modification").Value);

            var editorNode = config.Descendants(c + @"editor").FirstOrDefault(n => n.Attribute("name").Value == "Test.Editor");
            Assert.IsNotNull(editorNode, @"The editor node was not found in System.config.");
            Assert.AreEqual(@"Extensions\Extension Name\Editor", editorNode.Element(c + @"installpath").Value);
            Assert.AreEqual(@"Configuration\Editor.config", editorNode.Element(c + @"configuration").Value);
            Assert.AreEqual(@"Test", editorNode.Element(c + @"vdir").Value);

            var modelNode = config.Descendants(c + @"model").FirstOrDefault(n => n.Attribute("name").Value == "Test.Model");
            Assert.IsNotNull(modelNode, @"The model node was not found in System.config.");
            Assert.AreEqual(@"Extensions\Extension Name\Model", modelNode.Element(c + @"installpath").Value);
            Assert.AreEqual(@"Configuration\Model.config", modelNode.Element(c + @"configuration").Value);
            Assert.AreEqual(@"Test", modelNode.Element(c + @"vdir").Value);

            // Disable the extension again
            manager.Disable(extension);
            Assert.AreEqual(InstallState.Installed, extension.Status);
            Assert.IsFalse(extension.Enabled);

            // Validate that System.config has been updated to remove the extension
            config = XDocument.Load(systemConfig);
            serverNode = config.Descendants(c + @"server").FirstOrDefault();
            Assert.IsNotNull(serverNode);
            Assert.AreEqual("2", serverNode.Attribute("modification").Value);

            editorNode = config.Descendants(c + @"editor").FirstOrDefault(n => n.Attribute("name").Value == "Test.Editor");
            Assert.IsNull(editorNode, @"Editor node was still found in the configuration after disabling the extension.");
            modelNode = config.Descendants(c + @"model").FirstOrDefault(n => n.Attribute("name").Value == "Test.Model");
            Assert.IsNull(modelNode, @"Model node was still found in the configuration after disabling the extension.");
        }

        [TestMethod]
        public void TestCannotEnableUninstalledExtension()
        {
            var manager = new ExtensionManager(repository, installationDirectory, systemConfig);
            var extensions = manager.GetExtensions();
            Assert.AreEqual(numberOfExtensions, extensions.Count);

            var extension = extensions.FirstOrDefault(x => x.Name == "Extension Name");
            Assert.IsNotNull(extension, @"Failed to locate the test extension.");
            try
            {
                manager.Enable(extension);
                Assert.Fail(@"Attempting to enable an extension that isn't involved should give an error");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(@"You cannot enable an uninstalled extension.", ex.Message);
            }
        }
    }
}
