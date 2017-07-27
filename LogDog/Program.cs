// Prioritised requirements:
// TODO: Switch from ContextMenu to ContextMenuStrip.
// TODO: x Search function.
// TODO: x Shortcut keys in simple menu.
// TODO: x Icon for the 'exit' menu option.
// TODO: Support multiple host files.
// TODO: This class needs major refactoring.
// TODO: Optimise the file scanning logic.
// TODO: Filename matchign algorithm needs work (trailing numbers are assumed to be timestamps).
// TODO: Change icon when refreshing.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
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
    private static ContextMenu _detailedMenu;
    private static List<string> _favourites = new List<string>();
    private static bool _triggerRefresh;
    private static WindowsHostsFile _hostsFile;
    private static readonly object _buildContextMenuLock = new object();
    private static readonly object _refreshLock = new object();

    //-------------------------------------------------------------------------

    [STAThread]
    public static void Main()
    {
      try
      {
        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        InitialiseHostsFile();
        ExtractFavouritesFromSettings();

        _systemTrayIcon = new NotifyIcon
        {
          ContextMenu = new ContextMenu(),
          Icon = Resources.icon,
          Text = Application.ProductName,
          Visible = true
        };
        _systemTrayIcon.Click += OnIconClicked;

        AddScanningMenuItem(_systemTrayIcon.ContextMenu);
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

    private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
      var exception = e.ExceptionObject as Exception;

      MessageBox.Show(
        $"{exception?.Message}{Environment.NewLine}{Environment.NewLine}{exception?.StackTrace}",
        "Unhandled Exception",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
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

    private static void InitialiseHostsFile()
    {
      _hostsFile = new WindowsHostsFile(
        Environment.ExpandEnvironmentVariables(Settings.Default.HostsFilename),
        Settings.Default.HostFileBlockStart,
        Settings.Default.HostFileBlockEnd);

      _hostsFile.FileChanged += OnHostsFileChanged;
    }

    //-------------------------------------------------------------------------

    private static void ExtractFavouritesFromSettings()
    {
      _favourites = Settings.Default.Favourites.Split(';').ToList();
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
      var resetRefreshFlag = new ResetRefreshFlagDelegate(ResetRefreshFlag);

      _runnerIsAlive = true;

      while (_runnerIsAlive)
      {
        try
        {
          ContextMenu menu;
          BuildDetailedContextMenu(out menu);
          setContextMenu.Invoke(menu);

          for (var i = 0; i < 60 * 10; i++)
          {
            Thread.Sleep(1000);

            if (_triggerRefresh)
            {
              resetRefreshFlag.Invoke();
              break;
            }
          }
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
      _detailedMenu = menu;

      if (_systemTrayIcon == null)
      {
        return;
      }

      _systemTrayIcon.ContextMenu?.Dispose();
      _systemTrayIcon.ContextMenu = _detailedMenu;
    }

    //-------------------------------------------------------------------------

    private delegate void ResetRefreshFlagDelegate();

    private static void ResetRefreshFlag()
    {
      _triggerRefresh = false;
    }

    //-------------------------------------------------------------------------

    private static void BuildDetailedContextMenu(out ContextMenu detailedMenu)
    {
      lock (_buildContextMenuLock)
      {
        ContextMenu menu = new ContextMenu();

        string[] pathsToMonitor = Settings.Default.FoldersToMonitor.Split(';');

        var files = new VersionedFileCollection();

        foreach (var host in _hostsFile.Hosts)
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
          bool isFavourite = _favourites.Contains(file.Value.BaseFilename.ToLower());

          var subMenu = new VersionedFileMenu(file.Value, isFavourite);
          subMenu.Favourited += OnItemFavourited;
          subMenu.Unfavourited += OnItemUnfavourited;

          menu.MenuItems.Add(subMenu.MenuItem);
        }

        AddDefaultMenuOptions(menu);

        detailedMenu = menu;
      }
    }

    //-------------------------------------------------------------------------

    private static ContextMenu BuildSimpleMenu(ContextMenu detailedMenu)
    {
      ContextMenu simpleMenu = new ContextMenu();

      foreach (MenuItem menuItem in detailedMenu.MenuItems)
      {
        var versionedFileMenu = menuItem.Tag as VersionedFileMenu;

        if (versionedFileMenu != null &&
            versionedFileMenu.IsFavourite)
        {
          var clonedMenu = menuItem.CloneMenu();
          clonedMenu.MenuItems.Clear();

          // TODO: Find a better way to remove the * that indicates a favourite in the detailed menu.
          clonedMenu.Text = clonedMenu.Text.Replace("*", "");

          simpleMenu.MenuItems.Add(clonedMenu);
        }
      }

      if (simpleMenu.MenuItems.Count == 0)
      {
        simpleMenu.MenuItems.Add(
          new MenuItem
          {
            Text = "[No Favourites]",
            Enabled = false
          });
      }

      return simpleMenu;
    }

    //-------------------------------------------------------------------------

    private static void AddDefaultMenuOptions(ContextMenu menu)
    {
      AddRefreshOption(menu);
      AddExitOption(menu);
    }

    //-------------------------------------------------------------------------

    private static void AddRefreshOption(ContextMenu menu)
    {
      menu.MenuItems.Add("-");
      menu.MenuItems.Add(
        new MenuItem(
          "&Refresh",
          (sender, args) =>
          {
            PerformRefresh();
          }));
    }

    //-------------------------------------------------------------------------

    private static void AddExitOption(ContextMenu menu)
    {
      menu.MenuItems.Add("-");
      menu.MenuItems.Add(
        new MenuItem(
          "E&xit",
          (sender, args) => Shutdown()));
    }

    //-------------------------------------------------------------------------

    private static void AddScanningMenuItem(ContextMenu menu)
    {
      var menuItem = new MenuItem
      {
        Text = "Scanning...",
        Enabled = false
      };

      menuItem.Click += (sender, args) => Shutdown();
      menu.MenuItems.Add(menuItem);
    }

    //-------------------------------------------------------------------------

    private static void OnIconClicked(object sender, EventArgs args)
    {
      var mouseArgs = args as MouseEventArgs;

      if (mouseArgs?.Button == MouseButtons.Left)
      {
        if (_detailedMenu == null)
        {
          return;
        }

        _systemTrayIcon.ContextMenu = BuildSimpleMenu(_detailedMenu);

        typeof(NotifyIcon)
          .GetMethod(
            "ShowContextMenu",
            BindingFlags.Instance | BindingFlags.NonPublic)
          .Invoke(_systemTrayIcon, null);

        _systemTrayIcon.ContextMenu = _detailedMenu;
      }
    }

    //-------------------------------------------------------------------------

    private static void OnHostsFileChanged(object sender, EventArgs args)
    {
      PerformRefresh();
    }

    //-------------------------------------------------------------------------

    private static void PerformRefresh()
    {
      lock (_refreshLock)
      {
        _detailedMenu.MenuItems.Clear();
        AddScanningMenuItem(_detailedMenu);
        AddExitOption(_detailedMenu);
        _systemTrayIcon.ContextMenu = _detailedMenu;

        _triggerRefresh = true;
      }
    }

    //-------------------------------------------------------------------------

    private static void OnItemFavourited(object sender, EventArgs args)
    {
      var file = sender as VersionedFileMenu;

      if (file == null)
      {
        return;
      }

      var baseFilenameLower = file.File.BaseFilename.ToLower();

      if (_favourites.Contains(baseFilenameLower))
      {
        return;
      }

      _favourites.Add(baseFilenameLower);

      Settings.Default.Favourites = string.Join(";", _favourites.ToArray());
      Settings.Default.Save();
      Settings.Default.Reload();
    }

    //-------------------------------------------------------------------------

    private static void OnItemUnfavourited(object sender, EventArgs args)
    {
      var file = sender as VersionedFileMenu;

      if (file == null)
      {
        return;
      }

      var baseFilenameLower = file.File.BaseFilename.ToLower();

      if (_favourites.Contains(baseFilenameLower) == false)
      {
        return;
      }

      _favourites.Remove(baseFilenameLower);

      Settings.Default.Favourites = string.Join(";", _favourites.ToArray());
      Settings.Default.Save();
      Settings.Default.Reload();
    }

    //-------------------------------------------------------------------------
  }
}
