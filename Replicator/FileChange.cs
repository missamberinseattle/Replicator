using System.Text;

namespace Replicator
{
    public class FileChange
    {
        private static FileChange _empty;

        public FileChange(ChangeAction action, string file, int maxRetries)
        {
            Action = action;
            File = file;
            RetriesRemaining = maxRetries;
        }

        public ChangeAction Action { get; set; }
        public string File { get; set; }
        public int RetriesRemaining { get; set; }

        public static FileChange Empty
        {
            get
            {
                if (_empty == null)
                {
                    _empty = new FileChange(ChangeAction.None, string.Empty, -1);
                }

                return _empty;
            }
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            output.Append("{ ");
            output.AppendFormat("{0}; {1}; {2} retries remaining", Action, File, RetriesRemaining);
            output.Append(" }");
            return base.ToString();
        }
    }
}