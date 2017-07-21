// TODO: There is bug where if a valid host exists on the last line of the file it won't be picked up.

using System.IO;
using System.Reflection;
using NUnit.Framework;
using LogDog;

namespace LogDog_Test
{
  [TestFixture]
  [Category("WindowsHostsFile")]
  internal class WindowsHostsFile_Test
  {
    //-------------------------------------------------------------------------

    private WindowsHostsFile _testObject;

    //-------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
      var hostsFilenameAbs =
        $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{@"\TestResources\hosts"}";

      _testObject =
        new WindowsHostsFile(
          hostsFilenameAbs,
          "### Test Section",
          "###");
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ExceptionOnFileNotFound()
    {
      try
      {
        new WindowsHostsFile("someFileThatDoesntExist");
      }
      catch (FileNotFoundException)
      {
        Assert.Pass();
      }

      Assert.Fail();
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ExtractedCorrectNumberOfHostsFromFile()
    {
      Assert.AreEqual(2, _testObject.Hosts.Count);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ExtractedCorrectNumberOfHostsFromFileWhenNone()
    {
      var hostsFilenameAbs =
        $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{@"\TestResources\hostsWithoutTestSection"}";

      _testObject =
        new WindowsHostsFile(
          hostsFilenameAbs,
          "### Test Section",
          "###");

      Assert.AreEqual(0, _testObject.Hosts.Count);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ExtractedCorrectNumberOfHostsFromFileWhenNotFiltered()
    {
      var hostsFilenameAbs =
        $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{@"\TestResources\hostsTestSectionOnly"}";

      _testObject = new WindowsHostsFile(hostsFilenameAbs);

      Assert.AreEqual(2, _testObject.Hosts.Count);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ExtractedCorrectHostNamesFromFile()
    {
      Assert.True(_testObject.Hosts.ContainsKey("Name4"));
      Assert.True(_testObject.Hosts.ContainsKey("Name6"));
    }

    //-------------------------------------------------------------------------
  }
}
