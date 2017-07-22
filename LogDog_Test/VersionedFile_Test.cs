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
  }
}
