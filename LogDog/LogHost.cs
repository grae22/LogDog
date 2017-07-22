using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Net;

namespace LogDog
{
  internal class LogHost
  {
    //-------------------------------------------------------------------------

    public string Name { get; }
    public IPAddress Ip { get; }
    public IReadOnlyList<string> LogFilePaths { get; }

    private readonly List<string> _pathsToMonitor;
    private readonly string _filenameFilter;
    private readonly IFileSystem _fileSystem;
    private readonly List<string> _logFilePaths = new List<string>();

    //-------------------------------------------------------------------------

    public LogHost(string name,
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
      LogFilePaths = _logFilePaths;
    }

    //-------------------------------------------------------------------------

    public void RefreshLogFilePaths()
    {
      foreach (var path in _pathsToMonitor)
      {
        var pathWithoutLeadingOrTrailingPathSeparators =
          path
            .TrimStart('\\', '/')
            .TrimEnd('\\', '/');

        var fullPath = $@"\\{Ip}\{pathWithoutLeadingOrTrailingPathSeparators}\";

        _logFilePaths.AddRange(
          _fileSystem.Directory.GetFiles(
            fullPath,
            _filenameFilter));
      }
    }

    //-------------------------------------------------------------------------
  }
}
