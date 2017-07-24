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
      var file = new Mock<IFile>();
      file.SetupGet(x => x.Path).Returns("filename");
      file.SetupGet(x => x.HostName).Returns("host");

      string baseFilename = FilenameMatcher.ExtractHostQualifiedBaseFilename(file.Object);

      _testObject.AddFile(file.Object);

      Assert.True(_testObject.Files.ContainsKey(baseFilename));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void AddUnrelatedFiles()
    {
      var file1 = new Mock<IFile>();
      file1.SetupGet(x => x.Path).Returns("filename");
      file1.SetupGet(x => x.HostName).Returns("host1");

      var file2 = new Mock<IFile>();
      file2.SetupGet(x => x.Path).Returns("filename");
      file2.SetupGet(x => x.HostName).Returns("host2");

      string baseFilename1 = FilenameMatcher.ExtractHostQualifiedBaseFilename(file1.Object);
      string baseFilename2 = FilenameMatcher.ExtractHostQualifiedBaseFilename(file2.Object);

      _testObject.AddFile(file1.Object);
      _testObject.AddFile(file2.Object);

      Assert.True(_testObject.Files.ContainsKey(baseFilename1));
      Assert.True(_testObject.Files.ContainsKey(baseFilename2));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void AddRelatedFiles()
    {
      var file1 = new Mock<IFile>();
      file1.SetupGet(x => x.Path).Returns("filename_2017.1");
      file1.SetupGet(x => x.HostName).Returns("host");

      var file2 = new Mock<IFile>();
      file2.SetupGet(x => x.Path).Returns("filename_2017.2");
      file2.SetupGet(x => x.HostName).Returns("host");

      string baseFilename = FilenameMatcher.ExtractHostQualifiedBaseFilename(file1.Object);

      _testObject.AddFile(file1.Object);
      _testObject.AddFile(file2.Object);

      Assert.AreEqual(1, _testObject.Files.Count, "Should only be 1 versioned file instance as files are related.");
      Assert.True(_testObject.Files.ContainsKey(baseFilename));
    }

    //-------------------------------------------------------------------------
  }
}
