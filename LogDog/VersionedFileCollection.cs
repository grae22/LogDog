// TODO: AddFile() must handle base-filename casing.

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
      string baseFilename = FilenameMatcher.ExtractBaseFilename(file.Path);

      if (_files.ContainsKey(baseFilename) == false)
      {
        _files.Add(baseFilename, new VersionedFile(baseFilename));
      }

      _files[baseFilename].AddVersion(file);
    }

    //-------------------------------------------------------------------------
  }
}
