﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using MikeFactorial.XTB.PPCLIx;
using Moq;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.Control;

namespace MikeFactorial.XTB.Plugins.PPCLIx.Tests
{
    [TestClass]
    public class PPCLIxPluginControlTests
    {
        private PPCLIxPluginControl control = null;
        private List<NuGetVersion> versions = null;
        [TestInitialize]
        public void Init()
        {
            Form form = new Form();
            Mock<PacCLINugetFeed> nugetFeed = new Mock<PacCLINugetFeed>();
            versions = new List<NuGetVersion>();
            versions.Add(new NuGetVersion("1.30.7"));

            Mock<FindPackageByIdResource> resource = new Mock<FindPackageByIdResource>();
            resource.Setup(r => r.CopyNupkgToStreamAsync(It.IsAny<string>(), It.IsAny<NuGetVersion>(), It.IsAny<Stream>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string packageId, NuGetVersion version, Stream stream, SourceCacheContext cache, ILogger logger, CancellationToken token) => 
            {
                using (FileStream file = new FileStream("../../Nuget/microsoft.powerapps.cli.1.30.7.nupkg", FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    stream.Write(bytes, 0, (int)file.Length);
                }
                return true;
            });
            SourceRepository repository = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

            nugetFeed.Setup(n => n.Initialize());
            nugetFeed.Setup(n => n.Versions).Returns(versions);
            nugetFeed.Setup(n => n.Resource).Returns(resource.Object);
            nugetFeed.Setup(n => n.Repository).Returns(repository);
            control = new PPCLIxPluginControl(nugetFeed.Object);
            form.Controls.Add(control);

        }
        [TestMethod]
        public void CheckRunEnabled()
        {
            Assert.IsFalse(control.ToolStripRunButton.Enabled);
            control.TextBoxCommand.Text = "Test";
            control.textBoxCommandText_TextChanged(control.TextBoxCommand, null);
            Assert.IsFalse(control.ToolStripRunButton.Enabled);
            control.TextBoxCommand.Text = "pac ";
            control.textBoxCommandText_TextChanged(control.TextBoxCommand, null);
            Assert.IsTrue(control.ToolStripRunButton.Enabled);

        }
        [TestMethod]
        public void LoadVersionsTest()
        {
            control.TreePacCommands.BeforeExpand -= control.TreePacCommands_BeforeExpand;
            control.TreePacCommands.AfterSelect -= control.TreePacCommands_AfterSelect;
            control.ToolStripCLIVersionsDropDown.SelectedIndexChanged -= control.ToolStripCLIVersionsDropDown_SelectedIndexChanged;

            control.UpdatePacVersions();
            Assert.AreEqual(versions.Count, control.ToolStripCLIVersionsDropDown.Items.Count);

            control.TreePacCommands.BeforeExpand += control.TreePacCommands_BeforeExpand;
            control.TreePacCommands.AfterSelect += control.TreePacCommands_AfterSelect;
            control.ToolStripCLIVersionsDropDown.SelectedIndexChanged += control.ToolStripCLIVersionsDropDown_SelectedIndexChanged;

        }

        [TestMethod]
        public async Task InstallSelectedPacVersionTest()
        {
            control.TreePacCommands.BeforeExpand -= control.TreePacCommands_BeforeExpand;
            control.TreePacCommands.AfterSelect -= control.TreePacCommands_AfterSelect;
            control.ToolStripCLIVersionsDropDown.SelectedIndexChanged -= control.ToolStripCLIVersionsDropDown_SelectedIndexChanged;

            if (Directory.Exists("./microsoft.powerapps.cli.1.30.7"))
            {
                Directory.Delete("./microsoft.powerapps.cli.1.30.7", true);
            }
            control.UpdatePacVersions();
            await control.InstallSelectedPacVersion(control.NugetFeed.Versions.FirstOrDefault(v => v.OriginalVersion == "1.30.7"));
            Assert.IsTrue(Directory.Exists("./microsoft.powerapps.cli.1.30.7"));

            control.TreePacCommands.BeforeExpand += control.TreePacCommands_BeforeExpand;
            control.TreePacCommands.AfterSelect += control.TreePacCommands_AfterSelect;
            control.ToolStripCLIVersionsDropDown.SelectedIndexChanged += control.ToolStripCLIVersionsDropDown_SelectedIndexChanged;

        }
    }
}