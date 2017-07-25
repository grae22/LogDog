using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LogDog
{
  internal class VersionedFileMenu
  {
    //-------------------------------------------------------------------------

    public MenuItem MenuItem { get; } = new MenuItem();
    public bool IsFavourite { get; private set; }

    private readonly VersionedFile _file;

    //-------------------------------------------------------------------------

    public VersionedFileMenu(VersionedFile file,
                             bool isFavourite = false)
    {
      _file = file;
      _file.FileAdded += OnFileAdded;

      IsFavourite = isFavourite;

      BuildMenu();
    }

    //-------------------------------------------------------------------------
    
    private void BuildMenu()
    {
      MenuItem.Tag = this;
      MenuItem.Text = _file.BaseFilename;

      MenuItem.Click += (sender, args) => Process.Start(_file.FileVersions[0].Path);

      BuildSubMenus();
    }

    //-------------------------------------------------------------------------

    private void BuildSubMenus()
    {
      MenuItem.MenuItems.Clear();

      MenuItem.MenuItems.Add(
        new MenuItem(
          "Favourite?",
          (sender, args) =>
          {
            IsFavourite = !IsFavourite;
            MenuItem.MenuItems[0].Checked = IsFavourite;
          }));
      MenuItem.MenuItems[0].Checked = IsFavourite;

      foreach (FileInfo file in _file.FileVersions)
      {
        MenuItem.MenuItems.Add(
          new MenuItem(
            file.LastModified.ToString("yyyy-MM-dd HH:mm"),
            (sender, args) => { Process.Start(file.Path); }));
      }
    }

    //-------------------------------------------------------------------------

    private void OnFileAdded(object sender, EventArgs args)
    {
      BuildSubMenus();
    }

    //-------------------------------------------------------------------------
  }
}
