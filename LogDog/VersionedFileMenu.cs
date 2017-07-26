using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LogDog
{
  internal class VersionedFileMenu
  {
    //-------------------------------------------------------------------------

    public MenuItem MenuItem { get; } = new MenuItem();
    public VersionedFile File { get; }
    public bool IsFavourite { get; private set; }
    public event EventHandler Favourited;
    public event EventHandler Unfavourited;

    //-------------------------------------------------------------------------

    public VersionedFileMenu(VersionedFile file,
                             bool isFavourite = false)
    {
      File = file;
      File.FileAdded += OnFileAdded;

      IsFavourite = isFavourite;

      BuildMenu();
    }

    //-------------------------------------------------------------------------
    
    private void BuildMenu()
    {
      MenuItem.Tag = this;
      MenuItem.Text = File.BaseFilename;

      MenuItem.Click += (sender, args) => Process.Start(File.FileVersions[0].Path);

      BuildSubMenus();
    }

    //-------------------------------------------------------------------------

    private void BuildSubMenus()
    {
      MenuItem.MenuItems.Clear();

      AddFavouriteOption();

      foreach (FileInfo file in File.FileVersions)
      {
        MenuItem.MenuItems.Add(
          new MenuItem(
            file.LastModified.ToString("yyyy-MM-dd HH:mm"),
            (sender, args) => { Process.Start(file.Path); }));
      }
    }

    //-------------------------------------------------------------------------

    private void AddFavouriteOption()
    {
      MenuItem.MenuItems.Add(
        new MenuItem(
          "Favourite?",
          (sender, args) =>
          {
            IsFavourite = !IsFavourite;
            MenuItem.MenuItems[0].Checked = IsFavourite;

            if (IsFavourite)
            {
              Favourited?.Invoke(this, EventArgs.Empty);
            }
            else
            {
              Unfavourited?.Invoke(this, EventArgs.Empty);
            }
          }));

      MenuItem.MenuItems[0].Checked = IsFavourite;
    }

    //-------------------------------------------------------------------------

    private void OnFileAdded(object sender, EventArgs args)
    {
      BuildSubMenus();
    }

    //-------------------------------------------------------------------------
  }
}
