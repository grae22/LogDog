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
      try
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

        systemTrayIcon.Dispose();
      }
      catch (Exception e)
      {
        MessageBox.Show(
          $@"Unhandled exception ""{e.Message}""" +
          $@"{Environment.NewLine}{Environment.NewLine}" +
          $@"{e.StackTrace}",
          "Error",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }

    //-------------------------------------------------------------------------

    private static void BuildContextMenu(ContextMenu menu)
    {
      var hosts = new WindowsHostsFile(
        Environment.ExpandEnvironmentVariables(Settings.Default.HostsFilename),
        Settings.Default.HostFileBlockStart,
        Settings.Default.HostFileBlockEnd);

      string[] pathsToMonitor = Settings.Default.FoldersToMonitor.Split(';');

      var files = new VersionedFileCollection();

      foreach (var host in hosts.Hosts)
      {
        var fileHost = new FileHost(
          host.Key,
          host.Value,
          pathsToMonitor,
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
