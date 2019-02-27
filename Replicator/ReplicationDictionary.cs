using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Replicator
{
    public class ReplicationDictionary : Dictionary<string, ReplInfo>
    {
        #region Constructors, Destructors, and Initializers
        private ReplicationDictionary() : base()
        { }

        private ReplicationDictionary(int capacity) : base(capacity)
        { }

        private ReplicationDictionary(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
        { }

        private ReplicationDictionary(Dictionary<string, ReplInfo> dictionary) : base(dictionary)
        { }

        private ReplicationDictionary(IEqualityComparer<string> comparer) : base(comparer)
        { }

        private ReplicationDictionary(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }

        private ReplicationDictionary(IDictionary<string,ReplInfo> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
        { }
        #endregion

        public List<string> Roots { get; private set; }
        public SyncRules Rules { get; private set; }

        public static ReplicationDictionary BuildDictionary(List<string> roots, SyncRules rules)
        {
            var replDict = new ReplicationDictionary();
            replDict.Roots = roots;
            replDict.Rules = rules;

            var dirsToDo = new Stack<string>();

            for (var xx = 0; xx < roots.Count; xx++)
            {
                dirsToDo.Push(roots[xx]);
            }

            while (dirsToDo.Count > 0)
            {
                Application.DoEvents();
                var curDir = dirsToDo.Pop();
                
                var ruleResult = rules.ProcessPath(curDir);
                if (ruleResult == RuleResult.Reject)
                {
                    Logger.Verbose("Path ignored due to rule; " + curDir);
                    continue;
                }

                string curRoot = replDict.ExtractRootFromPath(curDir);

                var dirs = Directory.GetDirectories(curDir);

                for (var xx = 0; xx < dirs.Length; xx++)
                {
                    dirsToDo.Push(dirs[xx]);
                    Application.DoEvents();
                }

                var files = Directory.GetFiles(curDir);

                for (var xx = 0; xx < files.Length; xx++)
                {
                    var file = replDict.RemoveRoot(files[xx], curRoot);
                    Application.DoEvents();

                    ruleResult = rules.ProcessPath(file);

                    if (ruleResult == RuleResult.Reject)
                    {
                        Logger.Info("File rejected due to rule; " + file);
                        continue;
                    }

                    if (replDict.ContainsKey(file)) continue;

                    var replInfo = new ReplInfo(file, roots);

                    replDict.Add(file, replInfo);
                }
            }

            return replDict;
        }

        public string RemoveRoot(string path, string curRoot = null)
        {
            if (curRoot == null)
            {
                curRoot = ExtractRootFromPath(path);
            }

            var file = path.Substring(curRoot.Length);
            if (file.StartsWith("\\"))
            {
                file = file.Substring(1);
            }

            return file;
        }

        public string ExtractRootFromPath(string curDir)
        {
            string curRoot = null;

            for (var xx = 0; xx < Roots.Count; xx++)
            {
                if (!curDir.StartsWith(Roots[xx])) continue;

                curRoot = Roots[xx];
                break;
            }

            if (curRoot == null) throw new RootNotFoundException();

            return curRoot;
        }
    }
}
