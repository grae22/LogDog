using System;

namespace LogDog
{
  internal interface IFile
  {
    string Path { get; }
    string HostName { get; }
    DateTime LastModified { get; }
  }
}
