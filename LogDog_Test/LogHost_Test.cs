using System.IO.Abstractions;
using System.Net;
using System.Linq;
using NUnit.Framework;
using Moq;
using LogDog;

namespace LogDog_Test
{
  [TestFixture]
  [Category("Log")]
  internal class LogHost_Test
  {
    //-------------------------------------------------------------------------

    private LogHost _testObject;
    private Mock<IFileSystem> _fileSystem;

    //-------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
      _fileSystem = new Mock<IFileSystem>();

      _testObject =
        new LogHost(
          "TestObject",
          new IPAddress(new byte[] {1, 2, 3, 4}),
          new[] {@"\logs\"},
          "*.log",
          _fileSystem.Object);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void NameAndIp()
    {
      Assert.AreEqual("TestObject", _testObject.Name);
      Assert.AreEqual(new IPAddress(new byte[] {1, 2, 3, 4}), _testObject.Ip);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void LogFilePaths()
    {
      _fileSystem.Setup(
        x => x
          .Directory
          .GetFiles(It.IsAny<string>(), It.IsAny<string>()))
          .Returns(
            new[]
            {
              "file1.log",
              "file2.log"
            });

      _testObject.RefreshLogFilePaths();

      Assert.AreEqual(2, _testObject.LogFilePaths.Count);
      Assert.True(_testObject.LogFilePaths.Contains(@"file1.log"));
      Assert.True(_testObject.LogFilePaths.Contains(@"file2.log"));
    }

    //-------------------------------------------------------------------------
  }
}
