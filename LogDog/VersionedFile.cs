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
    public event EventHandler FileAdded;

    private readonly List<IFile> _fileVersions = new List<IFile>();

    //-------------------------------------------------------------------------

    public VersionedFile(string baseFilename)
    {
      FileVersions = _fileVersions;

      BaseFilename = baseFilename;
    }

    //-------------------------------------------------------------------------

    public void AddVersion(IFile file,
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
        _fileVersions.SingleOrDefault(
          x => x.Path.Equals(path, StringComparison.OrdinalIgnoreCase)) != null;
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
