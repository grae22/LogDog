using System;
using NUnit.Framework;
using LogDog;

namespace LogDog_Test
{
  internal class VersionedFileMenu_Test
  {
    //-------------------------------------------------------------------------

    private VersionedFileMenu _testObject;
    private VersionedFile _file;

    //-------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
      _file = new VersionedFile("SomeFilename");
      _testObject = new VersionedFileMenu(_file);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void MenuItemTextIsBaseFilename()
    {
      Assert.AreEqual("SomeFilename", _testObject.MenuItem.Text);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void SubMenuItemForEachFileVersion()
    {
      var file1 = new FileInfo
      {
        Path = "File1",
        HostName = "Host",
        LastModified = new DateTime(2017, 7, 24, 0, 0, 0)
      };

      var file2 = new FileInfo
      {
        Path = "File2",
        HostName = "Host",
        LastModified = new DateTime(2017, 7, 23, 0, 0, 0)
      };

      var file3 = new FileInfo
      {
        Path = "File3",
        HostName = "Host",
        LastModified = new DateTime(2017, 7, 21, 23, 59, 0)
      };

      _file.AddVersion(file1);
      _file.AddVersion(file2);
      _file.AddVersion(file3);

      Assert.AreEqual("Favourite?", _testObject.MenuItem.MenuItems[0].Text);
      Assert.AreEqual("2017-07-24 00:00", _testObject.MenuItem.MenuItems[1].Text);
      Assert.AreEqual("2017-07-23 00:00", _testObject.MenuItem.MenuItems[2].Text);
      Assert.AreEqual("2017-07-21 23:59", _testObject.MenuItem.MenuItems[3].Text);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ToggleFavourite()
    {
      _testObject.MenuItem.MenuItems[0].PerformClick();

      Assert.True(_testObject.IsFavourite);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void ToggleUnfavourite()
    {
      _testObject.MenuItem.MenuItems[0].PerformClick();
      _testObject.MenuItem.MenuItems[0].PerformClick();

      Assert.False(_testObject.IsFavourite);
    }

    //-------------------------------------------------------------------------

    [Test]
    public void EventRaisedOnFavourited()
    {
      _testObject.Favourited += (sender, args) => Assert.Pass();

      _testObject.MenuItem.MenuItems[0].PerformClick();

      Assert.Fail();
    }

    //-------------------------------------------------------------------------

    [Test]
    public void EventRaisedOnUnfavourited()
    {
      _testObject.Unfavourited += (sender, args) => Assert.Pass();

      _testObject.MenuItem.MenuItems[0].PerformClick();
      _testObject.MenuItem.MenuItems[0].PerformClick();

      Assert.Fail();
    }

    //-------------------------------------------------------------------------
  }
}
