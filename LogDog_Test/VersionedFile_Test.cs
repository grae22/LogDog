using System;
using System.Linq;
using NUnit.Framework;
using LogDog;

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
      var file = CreateFileInfo();

      _testObject.AddVersion(file);

      Assert.NotNull(
        _testObject.FileVersions.Single(
          x => x.Path == file.Path));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void PreventMultipleAdditionOfVersion()
    {
      var file = CreateFileInfo();

      _testObject.AddVersion(file);
      _testObject.AddVersion(file);

      Assert.AreEqual(1, _testObject.FileVersions.Count);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void SortNewestToOldest()
    {
      var fileOlder = CreateFileInfo(path: "abc", lastModified: new DateTime(0));
      var fileNewer = CreateFileInfo(path: "def", lastModified: new DateTime(1));

      _testObject.AddVersion(fileOlder);
      _testObject.AddVersion(fileNewer);

      Assert.AreEqual(1, _testObject.FileVersions[0].LastModified.Ticks);
      Assert.AreEqual(0, _testObject.FileVersions[1].LastModified.Ticks);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void EventRaisedWhenFileAdded()
    {
      var file = CreateFileInfo();

      _testObject.FileAdded += (sender, args) => { Assert.Pass(); };

      _testObject.AddVersion(file);

      Assert.Fail();
    }

    //-------------------------------------------------------------------------

    [Test]
    public void NoEventRaisedWhenFileAddedIfEventSuppressed()
    {
      var file = CreateFileInfo();

      _testObject.FileAdded += (sender, args) => { Assert.Fail(); };

      _testObject.AddVersion(file, true);

      Assert.Pass();
    }

    //=========================================================================

    private FileInfo CreateFileInfo(string path = "path",
                                    string hostName = "Host",
                                    DateTime? lastModified = null )
    {
      return new FileInfo()
      {
        Path = path,
        HostName = hostName,
        LastModified = lastModified ?? DateTime.Now
      };
    }

    //-------------------------------------------------------------------------
  }
}
