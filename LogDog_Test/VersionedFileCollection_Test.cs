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

      string baseFilename = FilenameMatcher.ExtractBaseFilename(file.Object.Path);

      _testObject.AddFile(file.Object);

      Assert.True(_testObject.Files.ContainsKey(baseFilename));
    }

    //-------------------------------------------------------------------------

    [Test]
    public void AddUnrelatedFiles()
    {
      var file1 = new Mock<IFile>();
      file1.SetupGet(x => x.Path).Returns("filenameA");

      var file2 = new Mock<IFile>();
      file2.SetupGet(x => x.Path).Returns("filenameB");

      string baseFilename1 = FilenameMatcher.ExtractBaseFilename(file1.Object.Path);
      string baseFilename2 = FilenameMatcher.ExtractBaseFilename(file2.Object.Path);

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

      var file2 = new Mock<IFile>();
      file2.SetupGet(x => x.Path).Returns("filename_2017.2");

      string baseFilename = FilenameMatcher.ExtractBaseFilename(file1.Object.Path);

      _testObject.AddFile(file1.Object);
      _testObject.AddFile(file2.Object);

      Assert.AreEqual(1, _testObject.Files.Count, "Should only be 1 versioned file instance as files are related.");
      Assert.True(_testObject.Files.ContainsKey(baseFilename));
    }

    //-------------------------------------------------------------------------
  }
}
