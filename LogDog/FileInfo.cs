using System;

namespace LogDog
{
  internal struct FileInfo
  {
    public string Path { get; set; }
    public string HostName { get; set; }
    public DateTime LastModified { get; set; }
  }
}
