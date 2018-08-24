using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using ListView = System.Windows.Controls.ListView;


namespace VSLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public CollectionViewSource ProjectsSource { get; set; } = new CollectionViewSource();

        private List<Project> Projects { get; set; } = new List<Project>();

        private List<string> Directories { get; set; }
        
        private const string Settings = "vsl-dirs.txt";
        private const string ProjectsSave = "projects.json";


        public MainWindow()
        {
            // Create settings file
            if (!File.Exists(Settings))
            {
                File.Create(Settings);
            }

            // Create projects file
            if (!File.Exists(ProjectsSave))
            {
                File.Create(ProjectsSave);
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
            
            CrawlDirectories();

            SProjects.Save(Projects, ProjectsSave);

            // Fill directories in settings
            DirectoriesTextBox.Text = String.Join(Environment.NewLine, Directories);

            // Fill ListView
            ProjectsControl.ItemsSource = Projects;
            CollectionView projectsView =
                (CollectionView) CollectionViewSource.GetDefaultView(ProjectsControl.ItemsSource);
            projectsView.Filter = ProjectFilter;
            // Sort by creation date
            projectsView.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Descending));
            projectsView.SortDescriptions.Add(new SortDescription("IsPinned", ListSortDirection.Descending));
            // Group by pinned
            projectsView.GroupDescriptions.Add(new PropertyGroupDescription("IsPinned"));
        }

        // Filter
        private bool ProjectFilter(object item)
        {
            if (String.IsNullOrEmpty(SearchTextbox.Text))
                return true;
            else
                return ((item as Project).Name.IndexOf(SearchTextbox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // Handle search
        private void SearchTextbox_OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            CollectionViewSource.GetDefaultView(ProjectsControl.ItemsSource).Refresh();
        }

        // Load projects
        public void LoadProjects()
        {
            Projects = SProjects.Read(ProjectsSave);
            
            ProjectsControl.ItemsSource = Projects;
            ProjectsSource.Source = Projects;

            CollectionViewSource.GetDefaultView(ProjectsControl.ItemsSource).Refresh();
        }

        // Crawl for projects
        public void CrawlDirectories()
        {
            Crawler c = new Crawler(Directories);
            var crawled = c.Crawl();

            foreach (var cr in crawled)
            {
                if (!Projects.Contains(cr)) Projects.Add(cr);
            }

            SProjects.Save(Projects, ProjectsSave);
            LoadProjects();
        }

        // Crawl directories
        private void CrawlBtn_OnClick(object sender, RoutedEventArgs e)
        {
            CrawlDirectories();
        }

        // Toggle settings panel
        private void SettingsBtn_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
            File.WriteAllText(Settings, DirectoriesTextBox.Text);

            if (SettingsFlyout.IsOpen)
                CrawlDirectories();
        }

        // Open project in VS
        private void OpenFile_Btn_OnClick(object sender, RoutedEventArgs e)
        {
            var file = (Project)(sender as ListView)?.SelectedItem;
            if (file != null) System.Diagnostics.Process.Start(file.Uri);
        }

        // Pin projects
        private void PinProject_Btn_Click(object sender, MouseButtonEventArgs e)
        {
            var file = (Project)(sender as ListView)?.SelectedItem;

            if (file != null)
            {
                file.IsPinned = true;
                Projects[Projects.IndexOf(file)].IsPinned = true;
                SProjects.Save(Projects, ProjectsSave);
                CollectionViewSource.GetDefaultView(ProjectsControl.ItemsSource).Refresh();
            }
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
                    CrawlDirectories();
                }
            }
        }

        // Handle window close
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            File.WriteAllText(Settings, DirectoriesTextBox.Text);
        }
    }
}
