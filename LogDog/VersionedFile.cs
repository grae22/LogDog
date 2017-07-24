using System;
using System.Collections.Generic;
using System.Linq;

namespace LogDog
{
  internal class VersionedFile
  {
    //-------------------------------------------------------------------------

    public string BaseFilename { get; }
    public IReadOnlyList<FileInfo> FileVersions { get; }
    public event EventHandler FileAdded;

    private readonly List<FileInfo> _fileVersions = new List<FileInfo>();

    //-------------------------------------------------------------------------

    public VersionedFile(string baseFilename)
    {
      FileVersions = _fileVersions;

      BaseFilename = baseFilename;
    }

    //-------------------------------------------------------------------------

    public void AddVersion(FileInfo file,
                           bool suppressFileAddedEvent = false)
    {
      if (ContainsFile(file.Path))
      {
        return;
      }

      _fileVersions.Add(file);

      Sort();

      if (suppressFileAddedEvent == false)
      {
        OnFileAdded();
      }
    }

    //-------------------------------------------------------------------------

    protected virtual void OnFileAdded()
    {
      FileAdded?.Invoke(this, EventArgs.Empty);
    }

    //-------------------------------------------------------------------------

    private bool ContainsFile(string path)
    {
      return
        _fileVersions.Count(
          x => x.Path.Equals(path, StringComparison.OrdinalIgnoreCase)) > 0;
    }

    //-------------------------------------------------------------------------

    private void Sort()
    {
      var sorted =
        _fileVersions
          .OrderByDescending(x => x.LastModified)
          .ToList();

      _fileVersions.Clear();
      _fileVersions.AddRange(sorted);
    }

    //-------------------------------------------------------------------------
  }
}
