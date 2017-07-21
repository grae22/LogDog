using System.Collections.Generic;
using System.IO.Abstractions;
using System.Net;

namespace LogDog
{
  internal class LogHost
  {
    //-------------------------------------------------------------------------

    public string Name { get; }
    public IPAddress Ip { get; }
    public IEnumerable<string> LogFilePaths { get; }

    private IEnumerable<string> _pathsToMonitor;
    private IFileSystem _fileSystem;

    //-------------------------------------------------------------------------

    public LogHost(string name,
                   IPAddress ip,
                   IEnumerable<string> pathsToMonitor,
                   IFileSystem fileSystem)
    {
      Name = name;
      Ip = ip;
      _pathsToMonitor = new List<string>( pathsToMonitor );
      _fileSystem = fileSystem;
    }

    //-------------------------------------------------------------------------



    //-------------------------------------------------------------------------
  }
}
