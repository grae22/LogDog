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

      BuildMenu();
    }

    //-------------------------------------------------------------------------

    public void Update()
    {
      BuildMenu();  // TODO: Rather have VersionedFile raise an event that this object will handle.
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

      foreach (IFile file in _file.FileVersions)
      {
        MenuItem.MenuItems.Add(
          new MenuItem(
            file.LastModified.ToString("yyyy-MM-dd HH:mm")));
      }
    }

    //-------------------------------------------------------------------------
  }
}
