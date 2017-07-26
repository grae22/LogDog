// TODO: This class needs major refactoring.
// TODO: Optimise the file scanning logic.
// TODO: Detect when the hosts file changes and reload everything.
// TODO: Icon for the 'exit' menu option.
// TODO: Search function.

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

    //-------------------------------------------------------------------------

    [STAThread]
    public static void Main()
    {
      try
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        ExtractFavouritesFromSettings();

        _systemTrayIcon = new NotifyIcon
        {
          ContextMenu = new ContextMenu(),
          Icon = Resources.icon,
          Text = Application.ProductName,
          Visible = true
        };
        _systemTrayIcon.Click += OnIconClicked;

        AddUpdatingMenuItem(_systemTrayIcon.ContextMenu);
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
          BuildContextMenus(out menu);
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

    private static void BuildContextMenus(out ContextMenu detailedMenu)
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
        bool isFavourite = _favourites.Contains(file.Value.BaseFilename.ToLower());

        var subMenu = new VersionedFileMenu(file.Value, isFavourite);
        subMenu.Favourited += OnItemFavourited;
        subMenu.Unfavourited += OnItemUnfavourited;

        menu.MenuItems.Add(subMenu.MenuItem);
      }
      
      AddDefaultMenuOptions(menu);

      detailedMenu = menu;
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
            _detailedMenu.MenuItems.Clear();
            AddUpdatingMenuItem(_detailedMenu);
            AddExitOption(_detailedMenu);
            _systemTrayIcon.ContextMenu = _detailedMenu;

            _triggerRefresh = true;
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

    private static void AddUpdatingMenuItem(ContextMenu menu)
    {
      var menuItem = new MenuItem
      {
        Text = "Updating...",
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
