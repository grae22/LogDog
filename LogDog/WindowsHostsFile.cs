// TODO: Register for events relating to the hosts file so we can reparse it when it changes.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;

namespace LogDog
{
  internal class WindowsHostsFile
  {
    //-------------------------------------------------------------------------

    public IReadOnlyDictionary<string, IPAddress> Hosts { get; }

    private Dictionary<string, IPAddress> _hosts = new Dictionary<string, IPAddress>();

    //-------------------------------------------------------------------------

    // Filter start & end texts can be null if filtering isn't required.

    public WindowsHostsFile(
      string hostsFilenameAbs,
      string filterStartText = null,
      string filterEndText = null)
    {
      Hosts = new ReadOnlyDictionary<string, IPAddress>(_hosts);

      ExtractHostsFromFile(hostsFilenameAbs, filterStartText, filterEndText);
    }

    //-------------------------------------------------------------------------

    private void ExtractHostsFromFile(
      string hostsFilenameAbs,
      string filterBlockStartText,
      string filterBlockEndText)
    {
      if (File.Exists(hostsFilenameAbs) == false)
      {
        throw new FileNotFoundException(
          "File not found.",
          hostsFilenameAbs);
      }

      string[] lines = File.ReadAllLines(hostsFilenameAbs);
      lines = GetEntriesWithinBlock(lines, filterBlockStartText, filterBlockEndText);
      PopulateHosts(lines);
    }

    //-------------------------------------------------------------------------

    // Block start & end texts can be null if filtering isn't required.

    private static string[] GetEntriesWithinBlock(
      string[] entries,
      string blockStartText,
      string blockEndText)
    {
      bool hasStartFilter = !string.IsNullOrEmpty(blockStartText);
      bool hasEndFilter = !string.IsNullOrEmpty(blockEndText);

      var blockStartTextLower = blockStartText?.ToLower();
      var blockEndTextLower = blockEndText?.ToLower();

      var blockStartIndex = -1;
      var blockEndIndex = -1;

      for (var i = 0; i < entries.Length; i++)
      {
        if (blockStartTextLower != null &&
            entries[i].ToLower().Contains(blockStartTextLower))
        {
          blockStartIndex = i + 1;  // We include from the next entry.
        }
        else if (blockStartIndex > -1 &&
                 blockEndTextLower != null &&
                 entries[i].ToLower().Contains(blockEndTextLower))
        {
          blockEndIndex = i - 1;  // We include up to and including the previous entry.
          break;
        }
      }

      // Bail if start/end were specified and not found.
      if (hasStartFilter && blockStartIndex < 0)
      {
        return new string[0];
      }

      if (hasEndFilter && blockEndIndex < 0)
      {
        return new string[0];
      }

      // Start/end filter not found? Use entire doc.
      if (blockStartIndex < 0)
      {
        blockStartIndex = 0;
      }

      if (blockEndIndex < 0)
      {
        blockEndIndex = entries.Length - 1;
      }

      // Start is after the end?
      if (blockStartIndex > blockEndIndex)
      {
        // TODO: An exception may be more appropriate than failing silently.
        return new string[0];
      }

      // Return the entries block.
      return
        entries
          .Skip(blockStartIndex)
          .Take(blockEndIndex - blockStartIndex)
          .ToArray();
    }

    //-------------------------------------------------------------------------

    private void PopulateHosts(string[] entries)
    {
      _hosts.Clear();

      foreach (string entry in entries)
      {
        IPAddress ip;
        string name;

        ExtractIpAndName(entry, out ip, out name);

        // TODO: Probably worth logging when duplicate keys encountered.
        if (name != null &&
            _hosts.ContainsKey(name) == false)
        {
          _hosts.Add(name, ip);
        }
      }
    }

    //-------------------------------------------------------------------------

    private void ExtractIpAndName(
      string entry,
      out IPAddress ip,
      out string name)
    {
      ip = null;
      name = null;

      string[] items = entry.Split(' ', '\t');

      if (items.Length < 2)
      {
        return;
      }

      var ipString = items[0];
      name = items[1];

      if (IPAddress.TryParse(ipString, out ip) == false)
      {
        name = null;
      }
    }

    //-------------------------------------------------------------------------
  }
}
