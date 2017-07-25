using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Windows.Forms;

namespace LogDog
{
  public static class Program
  {
    //-------------------------------------------------------------------------

    private static Thread _runner;
    private static bool _runnerIsAlive;
    private static NotifyIcon _systemTrayIcon;

    //-------------------------------------------------------------------------

    [STAThread]
    public static void Main()
    {
      try
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        _systemTrayIcon = new NotifyIcon()
        {
          ContextMenu = new ContextMenu(),
          Icon = Resources.icon,
          Text = Application.ProductName,
          Visible = true
        };

        AddExitOption(_systemTrayIcon.ContextMenu);

        InitialiseRunner();

        Application.Run();

        _systemTrayIcon.Dispose();
        _systemTrayIcon = null;
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

    private static void Shutdown()
    {
      _runnerIsAlive = false;

      if (_runner?.Join(1000) == false)
      {
        _runner.Abort();
      }

      Application.Exit();
    }

    //-------------------------------------------------------------------------

    private static void InitialiseRunner()
    {
      var threadStart = new ThreadStart(Run);
      _runner = new Thread(threadStart);
      _runner.Start();
    }

    //-------------------------------------------------------------------------

    private static void Run()
    {
      var setContextMenu = new SetContextMenuDelegate(SetContextMenu);

      _runnerIsAlive = true;

      while (_runnerIsAlive)
      {
        try
        {
          Cursor.Current = Cursors.WaitCursor;
          ContextMenu menu = BuildContextMenu();
          Cursor.Current = Cursors.Default;

          setContextMenu.Invoke(menu);

          Thread.Sleep(1000 * 60 * 10);
        }
        catch (ThreadInterruptedException)
        {
          // Ignore.
        }
      }
    }

    //-------------------------------------------------------------------------

    private delegate void SetContextMenuDelegate(ContextMenu menu);

    private static void SetContextMenu(ContextMenu menu)
    {
      if (_systemTrayIcon == null)
      {
        return;
      }

      _systemTrayIcon.ContextMenu?.Dispose();
      _systemTrayIcon.ContextMenu = menu;
    }

    //-------------------------------------------------------------------------

    private static ContextMenu BuildContextMenu()
    {
      ContextMenu menu = new ContextMenu();

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

      AddExitOption(menu);

      return menu;
    }

    //-------------------------------------------------------------------------

    private static void AddExitOption(ContextMenu menu)
    {
      menu.MenuItems.Add("-");
      menu.MenuItems.Add(
        new MenuItem(
          "E&xit",
          (sender, args) => { Shutdown(); }));
    }

    //-------------------------------------------------------------------------
  }
}
