using System;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ProjectFileTools.ToolWindows
{
    internal class ProjectShim
    {
        public ProjectShim(string name, string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Name = name;
            FileName = fileName;
        }

        public string FileName { get; }

        public string Name { get; }

        public static bool TryCreateProjectShim(IVsHierarchy hierarchy, out ProjectShim shim)
        {
            if (hierarchy is null)
            {
                shim = null;
                return false;
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out object projectNameObject);
            hierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out Guid type);
            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object extObj);

            string fileName;

            if (!(extObj is Project p))
            {
                hierarchy.GetCanonicalName(VSConstants.VSITEMID_ROOT, out fileName);
            }
            else
            {
                fileName = p.FileName;
            }

            string name = projectNameObject as string;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(fileName) || type == ProjectImportsViewModel.SolutionFolder)
            {
                shim = null;
                return false;
            }

            shim = new ProjectShim(name, fileName);
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectShim p && string.Equals(Name, p.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => Name.GetHashCode();
    }
}
