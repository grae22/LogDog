using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Net;

namespace LogDog
{
  internal class FileHost
  {
    //-------------------------------------------------------------------------

    public string Name { get; }
    public IPAddress Ip { get; }
    public IReadOnlyList<string> FilePaths { get; }

    private readonly List<string> _pathsToMonitor;
    private readonly string _filenameFilter;
    private readonly IFileSystem _fileSystem;
    private readonly List<string> _filePaths = new List<string>();

    //-------------------------------------------------------------------------

    public FileHost(string name,
                    IPAddress ip,
                    IEnumerable<string> pathsToMonitor,
                    string filenameFilter,
                    IFileSystem fileSystem)
    {
      Name = name;
      Ip = ip;
      _pathsToMonitor = new List<string>(pathsToMonitor);
      _filenameFilter = filenameFilter;
      _fileSystem = fileSystem;
      FilePaths = _filePaths;
    }

    //-------------------------------------------------------------------------

    public void RefreshFilePaths()
    {
      foreach (var path in _pathsToMonitor)
      {
        var pathWithoutLeadingOrTrailingPathSeparators =
          path
            .TrimStart('\\', '/')
            .TrimEnd('\\', '/');

        var fullPath = $@"\\{Ip}\{pathWithoutLeadingOrTrailingPathSeparators}\";

        if (_fileSystem.Directory.Exists(fullPath) == false)
        {
          continue;
        }

        _filePaths.AddRange(
          _fileSystem.Directory.EnumerateFiles(
            fullPath,
            _filenameFilter,
            SearchOption.AllDirectories));
      }
    }

    //-------------------------------------------------------------------------
  }
}
