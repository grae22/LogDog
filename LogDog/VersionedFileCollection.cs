using System.Collections.Generic;

namespace LogDog
{
  internal class VersionedFileCollection
  {
    //-------------------------------------------------------------------------

    public IReadOnlyDictionary<string, VersionedFile> Files { get; }

    private readonly Dictionary<string, VersionedFile> _files = new Dictionary<string, VersionedFile>();

    //-------------------------------------------------------------------------

    public VersionedFileCollection()
    {
      Files = _files;
    }

    //-------------------------------------------------------------------------

    public void AddFile(IFile file)
    {
      string baseFilenameLower = FilenameMatcher.ExtractHostQualifiedBaseFilename(file).ToLower();

      if (_files.ContainsKey(baseFilenameLower) == false)
      {
        _files.Add(baseFilenameLower, new VersionedFile(file.Path));
      }

      _files[baseFilenameLower].AddVersion(file);
    }

    //-------------------------------------------------------------------------
  }
}
