using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EnvDTE;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using ProjectFileTools.Helpers;
using ProjectFileTools.MSBuild;

namespace ProjectFileTools.ToolWindows
{
    public partial class ProjectImportsTreeControl
    {
        public ProjectImportsTreeControl()
        {
            InitializeComponent();
            this.ShouldBeThemed();
            ((FrameworkElement)FindResource("ImportElementMenu")).ShouldBeThemed();
        }

        public static object InactiveSelectionBackgroundColor => TreeViewColors.SelectedItemActiveColorKey;
        public static object InactiveSelectionTextColor => TreeViewColors.SelectedItemActiveTextColorKey;

        private void NavigateToImport(object sender, MouseButtonEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Label s = (Label)sender;
            string fullPath = (string)s.ToolTip;

            DTE dte = ServiceUtil.DTE;
            dte.MainWindow.Activate();

            if (!OpenProjectFileUtil.TryOpenFile(fullPath))
            {
                MessageBox.Show("Could not open file");
            }

            e.Handled = true;
        }

        private void NavigateToImport(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            MenuItem item = (MenuItem)sender;
            ImportTreeNode node = (ImportTreeNode)item.DataContext;

            DependencyObject target = ((ContextMenu)item.Parent).PlacementTarget;
            TreeViewItem tvi = target.GetParent<TreeViewItem>();
            if (tvi != null)
            {
                tvi.IsSelected = true;
            }

            if (!node.Self.HasValue)
            {
                return;
            }

            DTE dte = ServiceUtil.DTE;
            dte.MainWindow.Activate();

            if (!node.Self.HasValue)
            {
                return;
            }

            ElementLocation location = node.Self.Value.ImportingElement.ProjectLocation;

            if (!OpenProjectFileUtil.TryOpenFile(location.File))
            {
                MessageBox.Show("Could not open file");
            }
            else
            {
                try
                {
                    ((TextSelection)dte.ActiveDocument.Selection).GotoLine(location.Line, true);
                }
                catch
                {
                }

            }

            e.Handled = true;
        }

        private void NavigateToFile(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            MenuItem item = (MenuItem)sender;
            ImportTreeNode node = (ImportTreeNode)item.DataContext;

            DependencyObject target = ((ContextMenu)item.Parent).PlacementTarget;
            TreeViewItem tvi = target.GetParent<TreeViewItem>();
            if (tvi != null)
            {
                tvi.IsSelected = true;
            }

            DTE dte = ServiceUtil.DTE;
            dte.MainWindow.Activate();

            if (!OpenProjectFileUtil.TryOpenFile(node.FullPath))
            {
                MessageBox.Show("Could not open file");
            }

            e.Handled = true;
        }

        private void ShowInFileExplorer(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ImportTreeNode node = (ImportTreeNode)item.DataContext;

            DependencyObject target = ((ContextMenu)item.Parent).PlacementTarget;
            TreeViewItem tvi = target.GetParent<TreeViewItem>();
            if (tvi != null)
            {
                tvi.IsSelected = true;
            }

            System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{node.FullPath}\"");
        }
    }

    internal static class VisualTreeHelperExtensions
    {
        public static T GetParent<T>(this DependencyObject source)
            where T : DependencyObject
        {
            DependencyObject current = source;
            while (current != null)
            {
                if (current is T result)
                {
                    return result;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}