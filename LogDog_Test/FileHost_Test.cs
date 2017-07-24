using System.IO.Abstractions;
using System.Net;
using System.Linq;
using NUnit.Framework;
using Moq;
using LogDog;

namespace LogDog_Test
{
  [TestFixture]
  [Category("FileHost")]
  internal class FileHost_Test
  {
    //-------------------------------------------------------------------------

    private FileHost _testObject;
    private Mock<IFileSystem> _fileSystem;

    //-------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
      _fileSystem = new Mock<IFileSystem>();

      _testObject =
        new FileHost(
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
    public void FilePaths()
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

      _fileSystem.Setup(x => x.Directory.Exists(It.IsAny<string>())).Returns(true);

      _testObject.RefreshFilePaths();

      Assert.AreEqual(2, _testObject.FilePaths.Count);
      Assert.True(_testObject.FilePaths.Contains(@"file1.log"));
      Assert.True(_testObject.FilePaths.Contains(@"file2.log"));
    }

    //-------------------------------------------------------------------------
  }
}
