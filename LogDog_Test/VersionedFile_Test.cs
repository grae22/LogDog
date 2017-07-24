using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using NUnit.Framework;
using LogDog;
using Moq;

namespace LogDog_Test
{
  [TestFixture]
  [Category("VersionedFile")]
  internal class VersionedFile_Test
  {
    //-------------------------------------------------------------------------

    private VersionedFile _testObject;

    //-------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
      _testObject = new VersionedFile("TestObject");
    }

    //-------------------------------------------------------------------------

    [Test]
    public void BaseFilename()
    {
      Assert.AreEqual("TestObject", _testObject.BaseFilename);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void AddVersion()
    {
      var file = CreateMockFile();

      _testObject.AddVersion(file.Object);

      Assert.NotNull(
        _testObject.FileVersions.Single(
          x => x.Path == file.Object.Path));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void PreventMultipleAdditionOfVersion()
    {
      var file = CreateMockFile();

      _testObject.AddVersion(file.Object);
      _testObject.AddVersion(file.Object);

      Assert.AreEqual(1, _testObject.FileVersions.Count);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void SortNewestToOldest()
    {
      var fileOlder = CreateMockFile(path: "abc", lastModified: new DateTime(0));
      var fileNewer = CreateMockFile(path: "def", lastModified: new DateTime(1));

      _testObject.AddVersion(fileOlder.Object);
      _testObject.AddVersion(fileNewer.Object);

      Assert.AreSame(fileNewer.Object, _testObject.FileVersions[0]);
      Assert.AreSame(fileOlder.Object, _testObject.FileVersions[1]);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void EventRaisedWhenFileAdded()
    {
      var file = CreateMockFile();

      _testObject.FileAdded += (sender, args) => { Assert.Pass(); };

      _testObject.AddVersion(file.Object);

      Assert.Fail();
    }

    //-------------------------------------------------------------------------

    [Test]
    public void NoEventRaisedWhenFileAddedIfEventSuppressed()
    {
      var file = CreateMockFile();

      _testObject.FileAdded += (sender, args) => { Assert.Fail(); };

      _testObject.AddVersion(file.Object, true);

      Assert.Pass();
    }

    //=========================================================================

    private Mock<IFile> CreateMockFile(string path = "path",
                                       string hostName = "Host",
                                       DateTime? lastModified = null )
    {
      var mock = new Mock<IFile>();

      mock.SetupGet(x => x.Path).Returns(path);
      mock.SetupGet(x => x.HostName).Returns(hostName);
      mock.SetupGet(x => x.LastModified).Returns(lastModified ?? DateTime.Now);

      return mock;
    }

    //-------------------------------------------------------------------------
  }
}
