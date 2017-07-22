using System;

namespace LogDog
{
  internal interface IFile
  {
    string Path { get; }
    DateTime LastModified { get; }
  }
}
