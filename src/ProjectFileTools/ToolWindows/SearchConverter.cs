using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using ProjectFileTools.MSBuild;

namespace ProjectFileTools.ToolWindows
{
    internal class SearchConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[1] is string searchText) || string.IsNullOrEmpty(searchText) || !(values[0] is IReadOnlyList<ImportTreeNode> nodes))
            {
                return values[0];
            }

            return Filter(nodes, searchText);
        }

        private List<ImportTreeNode> Filter(IReadOnlyList<ImportTreeNode> nodes, string searchText)
        {
            List<ImportTreeNode> result = new List<ImportTreeNode>();

            foreach (ImportTreeNode node in nodes)
            {
                if (node.FullPath.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1 || Filter(node.Children, searchText).Count > 0)
                {
                    result.Add(node);
                }
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
