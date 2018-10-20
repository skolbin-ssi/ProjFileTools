using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ProjectFileTools.Helpers
{
    internal static class OpenProjectFileUtil
    {
        public static bool TryOpenFile(string fullPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            using (var state = new NewDocumentStateScope(__VSNEWDOCUMENTSTATE.NDS_Provisional, Guid.Parse(ProjectFileToolsPackage.PackageGuidString)))
            {
                try
                {
                    Guid csprojEditor = new Guid("ebc191fb-ab14-4a9f-b3a8-41093791991e");
                    Guid xmlEditorFactoryGuid = Guid.Parse("{fa3cd31e-987b-443a-9b81-186104e8dac1}");
                    Guid logicalView = VSConstants.LOGVIEWID.Primary_guid;

                    try
                    {
                        VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, fullPath);
                    }
                    catch
                    {
                        VsShellUtilities.OpenDocumentWithSpecificEditor(ServiceProvider.GlobalProvider, fullPath, csprojEditor, logicalView);
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
