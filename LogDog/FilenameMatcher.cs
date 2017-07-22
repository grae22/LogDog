using System;
using System.IO;
using System.Linq;

namespace LogDog
{
  internal class FilenameMatcher
  {
    //-------------------------------------------------------------------------

    public static string ExtractBaseFilename(string filename)
    {
      var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

      // We assume numbers are not important to the name (probably date/time),
      // so we just take the letters, dashes, underscores, etc.
      var tmp = string.Empty;

      filenameWithoutExtension
        .TakeWhile(x => char.IsLetter(x) || char.IsPunctuation(x))
        .ToList()
        .ForEach(x => tmp += x);

      // Remove any trailing dashes, underscores, etc.
      var tmpReversed = new string(tmp.Reverse().ToArray());

      tmpReversed =
        new string(
          tmpReversed.SkipWhile(char.IsPunctuation).ToArray());

      return
        new string(
          tmpReversed.Reverse().ToArray());
    }

    //-------------------------------------------------------------------------

    public string BaseFilename { get; }

    private readonly string _baseFilenameLower;

    //-------------------------------------------------------------------------

    public FilenameMatcher(string filename)
    {
      BaseFilename = ExtractBaseFilename(filename);

      _baseFilenameLower = BaseFilename.ToLower();
    }

    //-------------------------------------------------------------------------

    public bool Matches(string otherFilename)
    {
      var otherBaseFilename = ExtractBaseFilename(otherFilename);

      return BaseFilename.Equals(otherBaseFilename, StringComparison.OrdinalIgnoreCase);
    }

    //-------------------------------------------------------------------------
  }
}
