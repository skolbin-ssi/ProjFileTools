using System;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace ProjectFileTools.ToolWindows
{
    internal sealed class ProjectImportsTreeCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("116a38f1-286e-4002-bfb8-ee1174bcbc09");
        private readonly AsyncPackage _package;

        public ProjectImportsTreeCommand(AsyncPackage package)
        {
            _package = package;
        }

        public static ProjectImportsTreeCommand Instance { get; private set; }

        private IAsyncServiceProvider ServiceProvider => _package;

        public void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SelectedItems selectedItems = ServiceUtil.DTE.SelectedItems;
            IVsMonitorSelection monitorSelection = ServiceUtil.GetService<SVsShellMonitorSelection, IVsMonitorSelection>();

            monitorSelection.GetCurrentSelection(out IntPtr pHier, out _, out _, out _);
            IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(pHier, typeof(IVsHierarchy)) as IVsHierarchy;

            if (!ProjectShim.TryCreateProjectShim(selectedHierarchy, out ProjectShim shim))
            {
                return;
            }

            ToolWindowPane window = _package.FindToolWindow(typeof(ProjectImportsTree), 0, true);
            if (window?.Frame is null)
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            ProjectImportsViewModel.ActiveInstance.RefreshProjectsCommand.Execute(null);
            ProjectImportsViewModel.ActiveInstance.SelectedProject = shim;
        }
    }
}
