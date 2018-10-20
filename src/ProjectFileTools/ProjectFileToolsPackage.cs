using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using ProjectFileTools.ToolWindows;
using Task = System.Threading.Tasks.Task;

namespace ProjectFileTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ProjectFileTools.ToolWindows.ProjectImportsTree))]
    public sealed class ProjectFileToolsPackage : AsyncPackage
    {
        public const string PackageGuidString = "60347b36-f766-4480-8038-ff1b70212235";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
        
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            OleMenuCommandService commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            ProjectImportsTreeCommand command = new ProjectImportsTreeCommand(this);

            var menuCommandID = new CommandID(ProjectImportsTreeCommand.CommandSet, ProjectImportsTreeCommand.CommandId);
            var menuItem = new MenuCommand(command.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }
    }
}
