using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using VSLauncher.Properties;
using static System.String;


namespace VSLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<Project> ProjectsCollection { get; set; }
        private List<Project> Projects { get; }
        private List<string> Directories { get; set; }

        private Settings _settings = Settings.Default;

        public MainWindow()
        {
            // Move to bottom right
            SizeChanged += (o, e) =>
            {
                var r = SystemParameters.WorkArea;
                Left = r.Right - ActualWidth;
                Top = r.Bottom - ActualHeight;
            };
            
            // Load settings and sanitize list
            Directories = _settings.Directories.Where(s => !IsNullOrWhiteSpace(s)).Distinct().ToList();

            // INIT
            InitializeComponent();

            dbg.Text = Directories.Count().ToString();

            // load projects
            Crawler c = new Crawler(Directories);

            Projects = c.Crawl();

            ProjectsCollection = new ObservableCollection<Project>(Projects);

            // Fill directories in settings
            DirectoriesTextBox.Text = Join(Environment.NewLine, Directories);
        }

        // Open settings panel
        private void SettingsBtn_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        // Open project in VS
        private void OpenFile_Btn_OnClick(object sender, RoutedEventArgs e)
        {
            Project file = ((FrameworkElement)sender).DataContext as Project;
            //System.Diagnostics.Process.Start(file.Uri);
        }

        // Select directory
        private void SelectDirectoryBtn_OnClick(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DirectoriesTextBox.Text += folderBrowserDialog.SelectedPath + Environment.NewLine;
                    Directories = DirectoriesTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                    _settings.Directories = Directories;
                    _settings.Save();
                }
            }
        }

        // Handle search
        private void SearchTextbox_OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            ProjectsCollection = new ObservableCollection<Project>(Projects);

            foreach (Project p in Projects)
            {
                string filter = SearchTextbox.Text.ToLower().Replace(" ", "");
                string name = p.Name.ToLower().Replace(" ", "");

                Title = (name.Contains(filter)).ToString();

                if (!name.Contains(filter)) ProjectsCollection.Remove(p);
            }
        }
    }
}
