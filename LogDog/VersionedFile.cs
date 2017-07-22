using System;
using System.Collections.Generic;
using System.Linq;

namespace LogDog
{
  internal class VersionedFile
  {
    //-------------------------------------------------------------------------

    public string BaseFilename { get; }
    public IReadOnlyList<IFile> FileVersions { get; }

    private List<IFile> _fileVersions = new List<IFile>();

    //-------------------------------------------------------------------------

    public VersionedFile(string baseFilename)
    {
      FileVersions = _fileVersions;

      BaseFilename = baseFilename;
    }

    //-------------------------------------------------------------------------

    public void AddVersion(IFile file)
    {
      if (ContainsFile(file.Path))
      {
        return;
      }

      _fileVersions.Add(file);
    }

    //-------------------------------------------------------------------------

    private bool ContainsFile(string path)
    {
      return
        _fileVersions.SingleOrDefault(
          x => x.Path.Equals(path, StringComparison.OrdinalIgnoreCase)) != null;
    }

    //-------------------------------------------------------------------------
  }
}
