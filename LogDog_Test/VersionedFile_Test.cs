using System;
using System.Linq;
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
      var file = new Mock<IFile>();
      file.SetupGet(x => x.Path).Returns("abc");

      _testObject.AddVersion(file.Object);

      Assert.NotNull(
        _testObject.FileVersions.Single(
          x => x.Path == file.Object.Path));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void PreventMultipleAdditionOfVersion()
    {
      var file = new Mock<IFile>();
      file.SetupGet(x => x.Path).Returns("abc");

      _testObject.AddVersion(file.Object);
      _testObject.AddVersion(file.Object);

      Assert.AreEqual(1, _testObject.FileVersions.Count);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void SortNewestToOldest()
    {
      var fileOlder = new Mock<IFile>();
      fileOlder.SetupGet(x => x.Path).Returns("abc");
      fileOlder.SetupGet(x => x.LastModified).Returns(new DateTime(0));

      var fileNewer = new Mock<IFile>();
      fileNewer.SetupGet(x => x.Path).Returns("def");
      fileNewer.SetupGet(x => x.LastModified).Returns(new DateTime(1));

      _testObject.AddVersion(fileOlder.Object);
      _testObject.AddVersion(fileNewer.Object);

      Assert.AreSame(fileNewer.Object, _testObject.FileVersions[0]);
      Assert.AreSame(fileOlder.Object, _testObject.FileVersions[1]);
    }

    //-------------------------------------------------------------------------
  }
}
