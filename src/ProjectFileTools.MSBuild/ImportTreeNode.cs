using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;

namespace ProjectFileTools.MSBuild
{
    public class ImportTreeNode
    {
        public ImportTreeNode(ResolvedImport self, bool evaluatedToTrue)
        {
            Children = new List<ImportTreeNode>();
            Self = self;
            FullPath = self.ImportedProject.FullPath;
            FileName = Path.GetFileName(FullPath);
        }

        public ImportTreeNode(Project self)
        {
            Children = new List<ImportTreeNode>();
            FullPath = self.FullPath;
            FileName = Path.GetFileName(FullPath);
            IsRoot = true;
        }

        public string FileName { get; }

        public string FullPath { get; }

        public bool IsRoot { get; }

        public bool IsChild => !IsRoot;

        public ResolvedImport? Self { get; }

        public List<ImportTreeNode> Children { get; }

        public override string ToString()
        {
            return $"{FileName ?? "(Root)"} - ({Children.Count} children)";
        }
    }

}
