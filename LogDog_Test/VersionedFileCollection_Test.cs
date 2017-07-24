using System;
using NUnit.Framework;
using Moq;
using LogDog;

namespace LogDog_Test
{
  [TestFixture]
  [Category("VersionedFileCollection")]
  internal class VersionedFileCollection_Test
  {
    //-------------------------------------------------------------------------

    private VersionedFileCollection _testObject;

    //-------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
      _testObject = new VersionedFileCollection();
    }

    //-------------------------------------------------------------------------

    [Test]
    public void AddFile()
    {
      var file = CreateFileInfo();

      string baseFilename = FilenameMatcher.ExtractHostQualifiedBaseFilename(file).ToLower();

      _testObject.AddFile(file);

      Assert.True(_testObject.Files.ContainsKey(baseFilename));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void AddUnrelatedFiles()
    {
      var file1 = CreateFileInfo(hostName: "host1");
      var file2 = CreateFileInfo(hostName: "host2");

      string baseFilename1 = FilenameMatcher.ExtractHostQualifiedBaseFilename(file1).ToLower();
      string baseFilename2 = FilenameMatcher.ExtractHostQualifiedBaseFilename(file2).ToLower();

      _testObject.AddFile(file1);
      _testObject.AddFile(file2);

      Assert.True(_testObject.Files.ContainsKey(baseFilename1));
      Assert.True(_testObject.Files.ContainsKey(baseFilename2));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void AddRelatedFiles()
    {
      var file1 = CreateFileInfo("filename_2017.1");
      var file2 = CreateFileInfo("filename_2017.2");

      string baseFilename = FilenameMatcher.ExtractHostQualifiedBaseFilename(file1).ToLower();

      _testObject.AddFile(file1);
      _testObject.AddFile(file2);

      Assert.AreEqual(1, _testObject.Files.Count, "Should only be 1 versioned file instance as files are related.");
      Assert.True(_testObject.Files.ContainsKey(baseFilename));
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
