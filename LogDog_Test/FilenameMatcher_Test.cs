using NUnit.Framework;
using Moq;
using LogDog;

namespace LogDog_Test
{
  [TestFixture]
  [Category("FilenameMatcher")]
  class FilenameMatcher_Test
  {
    //-------------------------------------------------------------------------

    private FilenameMatcher _testObject;

    //-------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
      _testObject = new FilenameMatcher("Test-Object_2017-07-22.log");
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ExtractsBaseFilename()
    {
      Assert.AreEqual(
        "Test-Object",
        FilenameMatcher.ExtractBaseFilename("Test-Object_2017-07-22.log"));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ExtractsHostQualifiedBaseFilename()
    {
      var file = new FileInfo()
      {
        Path = "Test-Object",
        HostName = "Host"
      };

      Assert.AreEqual(
        "(Host) Test-Object",
        FilenameMatcher.ExtractHostQualifiedBaseFilename(file));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void BaseFilename()
    {
      Assert.AreEqual("Test-Object", _testObject.BaseFilename);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ShouldMatch()
    {
      Assert.True(_testObject.Matches("test-object_2017-07-23.log"));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ShouldNotMatch()
    {
      Assert.False(_testObject.Matches("test-object-special_2017-07-23.log"));
    }

    //-------------------------------------------------------------------------
  }
}
