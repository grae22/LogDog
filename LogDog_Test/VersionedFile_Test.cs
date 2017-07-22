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
  }
}
