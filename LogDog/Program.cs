using System;
using System.IO;
using System.IO.Abstractions;
using System.Windows.Forms;

namespace LogDog
{
  public static class Program
  {
    //-------------------------------------------------------------------------

    [STAThread]
    public static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      var systemTrayIcon = new NotifyIcon()
      {
        ContextMenu = new ContextMenu(),
        Icon = Resources.icon,
        Text = Application.ProductName,
        Visible = true
      };

      BuildContextMenu(systemTrayIcon.ContextMenu);

      Application.Run();
    }

    //-------------------------------------------------------------------------

    private static void BuildContextMenu(ContextMenu menu)
    {
      var hosts = new WindowsHostsFile(
        Settings.Default.HostsFilename,
        Settings.Default.HostFileBlockStart,
        Settings.Default.HostFileBlockEnd);
        //$@"{Environment.GetEnvironmentVariable("windir")}\system32\drivers\etc\hosts",
        //"#-- Short names",
        //"#--");

      string[] pathsToMonitor = Settings.Default.FoldersToMonitor.Split(';');

      var files = new VersionedFileCollection();

      foreach (var host in hosts.Hosts)
      {
        var fileHost = new FileHost(
          host.Key,
          host.Value,
          pathsToMonitor,//new[] {@"c$\MGSLog", @"m$\MGSLog"},
          Settings.Default.FilenameFilter,
          new FileSystem());

        fileHost.RefreshFilePaths();

        var now = DateTime.Now;

        foreach (var filePath in fileHost.FilePaths)
        {
          DateTime lastModified = File.GetLastWriteTime(filePath);

          if ((now - lastModified).TotalDays > Settings.Default.HistoryInDays)
          {
            continue;
          }

          files.AddFile(
            new FileInfo
            {
              Path = filePath,
              HostName = host.Key,
              LastModified = lastModified
            },
            true);
        }
      }

      foreach (var file in files.Files)
      {
        var subMenu = new VersionedFileMenu(file.Value);
        menu.MenuItems.Add(subMenu.MenuItem);
      }

      menu.MenuItems.Add("-");
      menu.MenuItems.Add(
        new MenuItem(
          "E&xit",
          (sender, args) => { Application.Exit(); }));
    }

    //-------------------------------------------------------------------------
  }
}
