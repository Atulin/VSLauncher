using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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


namespace VSLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<Project> ProjectsCollection { get; set; }
        private List<Project> Projects { get; set; }
        private List<string> Directories { get; set; } = new List<string>();
        
        private const string Settings = "vsl-dirs.txt";


        public MainWindow()
        {
            // Create settings file
            if (!File.Exists(Settings))
            {
                File.Create(Settings);
            }

            Directories = File.ReadAllLines(Settings).ToList();
            
            // Move to bottom right
            SizeChanged += (o, e) =>
            {
                var r = SystemParameters.WorkArea;
                Left = r.Right - ActualWidth;
                Top = r.Bottom - ActualHeight;
            };
            
            // INIT
            InitializeComponent();
            
            LoadProjects();

            // Fill directories in settings
            DirectoriesTextBox.Text = String.Join(Environment.NewLine, Directories);
        }

        // Load projects
        public void LoadProjects()
        {
            Crawler c = new Crawler(Directories);
            Projects = c.Crawl();

            ProjectsCollection = new ObservableCollection<Project>(Projects);

            ProjectsControl.ItemsSource = null;
            ProjectsControl.ItemsSource = ProjectsCollection;
        }

        // Toggle settings panel
        private void SettingsBtn_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
            File.WriteAllText(Settings, DirectoriesTextBox.Text);
        }

        // Open project in VS
        private void OpenFile_Btn_OnClick(object sender, RoutedEventArgs e)
        {
            Project file = ((FrameworkElement)sender).DataContext as Project;
            System.Diagnostics.Process.Start(file.Uri);
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
                    File.WriteAllText(Settings, DirectoriesTextBox.Text);
                    LoadProjects();
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
                
                if (!name.Contains(filter)) ProjectsCollection.Remove(p);
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            File.WriteAllText(Settings, DirectoriesTextBox.Text);
        }
    }
}
