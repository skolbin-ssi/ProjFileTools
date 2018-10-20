using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ProjectFileTools.MSBuild;

namespace ProjectFileTools.ToolWindows
{
    internal class ProjectImportsViewModel : INotifyPropertyChanged
    {
        public static readonly Guid SolutionFolder = new Guid("{6bb5f8f0-4483-11d3-8bcf-00c04f8ec28c}");
        private readonly DTE _dte;
        private readonly Action<string> _setTitle;
        private readonly IWorkspaceManager _workspaceManager;
        private string _searchText;
        private IReadOnlyList<ImportTreeNode> _projectRootArray;
        private IReadOnlyList<ProjectShim> _projects;
        private ProjectShim _selectedProject;

        public ProjectImportsViewModel(Action<string> setTitle)
        {
            _setTitle = setTitle;
            _searchText = "";
            _dte = ServiceUtil.DTE;
            IComponentModel componentModel = ServiceUtil.GetService<SComponentModel, IComponentModel>();
            _workspaceManager = componentModel.DefaultExportProvider.GetExport<IWorkspaceManager>().Value;

            Projects = new ProjectShim[0];
            ProjectRootAsArray = new ImportTreeNode[0];
            RefreshProjectsCommand = ActionCommand.From(RefreshProjects);
            RefreshProjects();
            ActiveInstance = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static ProjectImportsViewModel ActiveInstance { get; private set; }

        public IReadOnlyList<ImportTreeNode> ProjectRootAsArray
        {
            get => _projectRootArray;
            set => Set(ref _projectRootArray, value);
        }

        public IReadOnlyList<ProjectShim> Projects
        {
            get => _projects;
            set => Set(ref _projects, value);
        }

        public ImageSource RefreshImage { get; } = WpfUtil.MonikerToBitmap(KnownMonikers.Refresh, 16);

        public ImageSource SearchImage { get; } = WpfUtil.MonikerToBitmap(KnownMonikers.Search, 16);

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value, StringComparer.OrdinalIgnoreCase, 200);
        }

        public ICommand RefreshProjectsCommand { get; }

        public ProjectShim SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (Set(ref _selectedProject, value))
                {
                    ProjectRootAsArray = new ImportTreeNode[0];

                    if (!(value is null))
                    {
                        IWorkspace workspace = _workspaceManager.GetWorkspace(_selectedProject.FileName);
                        ImportTreeNode root = workspace.ComputeImportTree(_workspaceManager);
                        ProjectRootAsArray = !(root is null) ? new[] { root } : new ImportTreeNode[0];
                        _setTitle(value.Name);
                    }
                    else
                    {
                        _setTitle("(No Project)");
                    }
                }
            }
        }

        private void RefreshProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsSolution solution = ServiceUtil.GetService<IVsSolution>();
            Guid empty = new Guid();
            if (!_dte.Solution.IsOpen || VSConstants.S_OK != solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, ref empty, out IEnumHierarchies hierEnum))
            {
                Projects = new ProjectShim[0];
                return;
            }

            List<ProjectShim> projects = new List<ProjectShim>();

            IVsHierarchy[] hiers = new IVsHierarchy[1];

            while (hierEnum.Next(1, hiers, out uint fetchCount) == VSConstants.S_OK && fetchCount > 0)
            {
                if (ProjectShim.TryCreateProjectShim(hiers[0], out ProjectShim shim))
                {
                    projects.Add(shim);
                }
            }

            projects.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
            Projects = projects;
        }

        private readonly ConcurrentDictionary<string, Guid> _propertiesPendingRaise = new ConcurrentDictionary<string, Guid>();

        private bool Set<T>(ref T target, T value, IEqualityComparer<T> comparer = null, int delay = 0, [CallerMemberName]string propertyName = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;

            if (!comparer.Equals(target, value))
            {
                target = value;

                if (delay > 0)
                {
                    Guid g = Guid.NewGuid();
                    _propertiesPendingRaise.AddOrUpdate(propertyName, g, (p, x) => g);
                    System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Run(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(delay);

                        if (_propertiesPendingRaise.TryGetValue(propertyName, out Guid g2) && g2 == g)
                        {
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        }
                    });
                }
                else
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }

                return true;
            }

            return false;
        }
    }
}
