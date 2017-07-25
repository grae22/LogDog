using System.Collections.Generic;

namespace LogDog
{
  internal class VersionedFileCollection
  {
    //-------------------------------------------------------------------------

    public IReadOnlyDictionary<string, VersionedFile> Files { get; }

    private readonly SortedDictionary<string, VersionedFile> _files = new SortedDictionary<string, VersionedFile>();

    //-------------------------------------------------------------------------

    public VersionedFileCollection()
    {
      Files = _files;
    }

    //-------------------------------------------------------------------------

    public void AddFile(FileInfo file,
                        bool suppressFileAddedEvent = false)
    {
      string qualifiedBaseFilename = FilenameMatcher.ExtractHostQualifiedBaseFilename(file);
      string qualifiedBaseFilenameLower = qualifiedBaseFilename.ToLower();

      if (_files.ContainsKey(qualifiedBaseFilenameLower) == false)
      {
        _files.Add(qualifiedBaseFilenameLower, new VersionedFile(qualifiedBaseFilename));
      }

      _files[qualifiedBaseFilenameLower].AddVersion(file, suppressFileAddedEvent);
    }

    //-------------------------------------------------------------------------
  }
}
