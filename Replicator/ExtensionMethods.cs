using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replicator
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this FileSystemEventArgs e)
        {
            var output = new StringBuilder();

            output.Append("{ ");
            output.AppendFormat("{2}: {0}; {1}", e.Name, e.FullPath, e.ChangeType);
            output.Append(" }");

            return output.ToString();
        }

        public static string ToFormattedString(this RenamedEventArgs e)
        {
            var output = new StringBuilder();

            output.Append("{ ");
            output.AppendFormat("{4}: {0}; {1} -> {2}; {3}", e.OldName, e.OldFullPath, e.FullPath, e.OldFullPath, e.ChangeType);
            output.Append(" }");

            return output.ToString();
        }
    }
}
