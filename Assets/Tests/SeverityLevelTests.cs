using NUnit.Framework;
using System;
using Unity.Services.DeploymentApi.Editor; // The namespace for your enum

namespace Unity.Services.DeploymentApi.Editor.Tests
{
    public class SeverityLevelTests
    {
        [Test]
        public void SeverityLevel_HasCorrectValues()
        {
            Assert.AreEqual(0, (int)SeverityLevel.None);
            Assert.AreEqual(1, (int)SeverityLevel.Info);
            Assert.AreEqual(2, (int)SeverityLevel.Warning);
            Assert.AreEqual(3, (int)SeverityLevel.Error);
            Assert.AreEqual(4, (int)SeverityLevel.Success);
        }
    }
}
