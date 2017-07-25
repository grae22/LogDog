﻿// TODO: This class needs major refactoring.
// TODO: Optimise the file scanning logic.
// TODO: Detect when the hosts file changes and reload everything.
// TODO: Icon for the 'exit' menu option.

using System;
using System.Collections.Generic;
using System.Drawing;
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

    //-------------------------------------------------------------------------

    [STAThread]
    public static void Main()
    {
      try
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        ExtractFavouritesFromSettings();

        _systemTrayIcon = new NotifyIcon()
        {
          ContextMenu = new ContextMenu(),
          Icon = new Icon(Resources.icon, new Size(32, 32)),
          Text = Application.ProductName,
          Visible = true
        };
        _systemTrayIcon.Click += OnIconClicked;

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

      _runnerIsAlive = true;

      while (_runnerIsAlive)
      {
        try
        {
          Cursor.Current = Cursors.WaitCursor;

          ContextMenu menu;
          BuildContextMenus(out menu);
          setContextMenu.Invoke(menu);

          Cursor.Current = Cursors.Default;

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
      _detailedMenu = menu;

      if (_systemTrayIcon == null)
      {
        return;
      }

      _systemTrayIcon.ContextMenu?.Dispose();
      _systemTrayIcon.ContextMenu = _detailedMenu;
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
        menu.MenuItems.Add(subMenu.MenuItem);
      }

      AddExitOption(menu);

      detailedMenu = menu;
    }

    //-------------------------------------------------------------------------

    private static ContextMenu BuildSimpleMenu(ContextMenu detailedMenu)
    {
      _favourites.Clear();

      ContextMenu simpleMenu = new ContextMenu();

      foreach (MenuItem menuItem in detailedMenu.MenuItems)
      {
        var versionedFileMenu = menuItem.Tag as VersionedFileMenu;

        if (versionedFileMenu != null &&
            versionedFileMenu.IsFavourite)
        {
          var clonedMenu = menuItem.CloneMenu();
          clonedMenu.MenuItems.Clear();
          simpleMenu.MenuItems.Add(clonedMenu);

          _favourites.Add(versionedFileMenu.MenuItem.Text.ToLower());
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

      Settings.Default.Favourites = string.Join(";", _favourites.ToArray());
      Settings.Default.Save();
      Settings.Default.Reload();

      return simpleMenu;
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
  }
}
