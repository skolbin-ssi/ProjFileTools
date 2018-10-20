using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace ProjectFileTools.ToolWindows
{
    [Guid("6f3a0c44-c544-4845-a284-e812c0ee7af9")]
    public class ProjectImportsTree : ToolWindowPane
    {
        public ProjectImportsTree() : base(null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Caption = "Import Visualizer - (No Project)";
            FrameworkElement content = new ProjectImportsTreeControl();
            SelectedItems selectedItems = ServiceUtil.DTE.SelectedItems;
            Project selectedProject = selectedItems.OfType<SelectedItem>().Select(x => x.Project).FirstOrDefault(x => !(x is null));

            ProjectImportsViewModel viewModel = new ProjectImportsViewModel(s => Caption = $"Import Visualizer - {s}")
            {
                SelectedProject = selectedProject != null ? new ProjectShim(selectedProject.Name, selectedProject.FileName) : null
            };

            content.DataContext = viewModel;
            Content = content;
        }
    }
}
