using System;
using System.Windows.Forms;

namespace LogDog
{
  internal class VersionedFileMenu
  {
    //-------------------------------------------------------------------------

    public MenuItem MenuItem { get; }

    private readonly VersionedFile _file;

    //-------------------------------------------------------------------------

    public VersionedFileMenu(VersionedFile file)
    {
      MenuItem = new MenuItem();
      _file = file;

      _file.FileAdded += OnFileAdded;

      BuildMenu();
    }

    //-------------------------------------------------------------------------
    
    private void BuildMenu()
    {
      MenuItem.Text = _file.BaseFilename;

      BuildSubMenus();
    }

    //-------------------------------------------------------------------------

    private void BuildSubMenus()
    {
      MenuItem.MenuItems.Clear();

      foreach (FileInfo file in _file.FileVersions)
      {
        MenuItem.MenuItems.Add(
          new MenuItem(
            file.LastModified.ToString("yyyy-MM-dd HH:mm")));
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
