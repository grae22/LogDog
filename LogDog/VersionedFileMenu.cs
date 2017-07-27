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

      UpdateMenuItemName();

      MenuItem.Click += ( sender, args ) => Process.Start( File.FileVersions[ 0 ].Path );

      BuildSubMenus();
    }

    //-------------------------------------------------------------------------

    private void UpdateMenuItemName()
    {
      var ageString = "";

      if (File.FileVersions.Count > 0)
      {
        TimeSpan age = DateTime.Now - File.FileVersions[0].LastModified;

        if (age.TotalDays > 0.99)
        {
          ageString = ">1 day";
        }
        else if (age.TotalHours > 0.99)
        {
          ageString = ">1 hr";
        }
        else if (age.TotalMinutes > 10)
        {
          ageString = ">10 mins";
        }
      }

      MenuItem.Text = $@"{(IsFavourite ? "*" : "")}{File.BaseFilename}";

      if (ageString.Length > 0)
      {
        MenuItem.Text += $"\t[{ageString}]";
      }
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

            UpdateMenuItemName();

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
