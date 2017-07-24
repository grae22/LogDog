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

    public void AddFile(FileInfo file,
                        bool suppressFileAddedEvent = false)
    {
      string baseFilename = FilenameMatcher.ExtractBaseFilename(file.Path);
      string baseFilenameLower = FilenameMatcher.ExtractHostQualifiedBaseFilename(file).ToLower();

      var qualifiedBaseFilename = $"{file.HostName}.{baseFilename}";

      if (_files.ContainsKey(baseFilenameLower) == false)
      {
        _files.Add(baseFilenameLower, new VersionedFile(qualifiedBaseFilename));
      }

      _files[baseFilenameLower].AddVersion(file, suppressFileAddedEvent);
    }

    //-------------------------------------------------------------------------
  }
}
