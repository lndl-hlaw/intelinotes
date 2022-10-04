using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace InteliNotes
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Directory.CreateDirectory(Constants.MainPath);
            Directory.CreateDirectory(Constants.NotebooksPath);
            if (!File.Exists(Constants.OpenedFile))
            {
                File.Create(Constants.OpenedFile);
            }
        }
    }
}
