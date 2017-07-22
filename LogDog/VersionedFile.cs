namespace LogDog
{
  internal class VersionedFile
  {
    //-------------------------------------------------------------------------

    public string BaseFilename { get; }

    //-------------------------------------------------------------------------

    public VersionedFile(string baseFilename)
    {
      BaseFilename = baseFilename;
    }

    //-------------------------------------------------------------------------
  }
}
