using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Replicator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private List<string> _roots = new List<string>();
        private List<FileSystemWatcher> _rootWatchers = new List<FileSystemWatcher>();
        private SyncRules _rules = null;
        private Dictionary<string, ReplInfo> _replDictionary = null;
        private ChangeQueue _changeQueue = new ChangeQueue();
        private static Queue<LogMessage> _loggerPump = new Queue<LogMessage>();
        private bool _processing = false;

        public const int MaxRetries = 100;

        public SyncRules Rules
        {
            get
            {
                if (_rules == null) _rules = new SyncRules();

                return _rules;
            }
        }

        public TraceLevel UiTraceLevel
        {
            get
            {
                var level = TraceLevel.Verbose;

                var text = CmbLogLevel.Items[CmbLogLevel.SelectedIndex].ToString();

                if (text == "Off") level = TraceLevel.Off;
                else if (text == "Verbose") level = TraceLevel.Verbose;
                else if (text == "Info") level = TraceLevel.Info;
                else if (text == "Warning") level = TraceLevel.Warning;
                else if (text == "Error") level = TraceLevel.Error;

                return level;
            }
        }

        public bool AutoStart { get; internal set; }

        public const int MaxOnScreenLogLength = 1024 * 1024; // 1mb of characters

        public static void AddToLogView(TraceLevel traceLevel, string prefix, string message)
        {
            _loggerPump.Enqueue(new LogMessage(traceLevel, prefix, message));
        }

        private void BtnStartSync_Click(object sender, EventArgs e)
        {
            BtnStartSync.Enabled = false;
            BtnEndSync.Enabled = true;

            InitRoots();
            Logger.Info("Building replication dictionary...");
            _replDictionary = ReplicationDictionary.BuildDictionary(_roots, Rules);

            Logger.Info("Queueing initial sync operations...");
            var count = 0;
            PbOpStatus.Maximum = _replDictionary.Count;
            PbOpStatus.Value = 0;

            foreach (var item in _replDictionary)
            {
                count++;
                _changeQueue.Queue(new FileChange(ChangeAction.Sync, item.Key, MaxRetries));
                PbOpStatus.Value = count;
                Application.DoEvents();
            }

            Logger.Info("Creating file system watchers...");
            for (var xx = 0; xx < _roots.Count; xx++)
            {
                var watcher = new FileSystemWatcher();
                var root = _roots[xx];

                watcher.Path = root;
                watcher.IncludeSubdirectories = true;

                // watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;

                watcher.Filter = "*.*";
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                watcher.EnableRaisingEvents = true;

                _rootWatchers.Add(watcher);
            }

            QueueTimer.Enabled = true;

        }

        private void InitRoots()
        {
            _rootWatchers.Clear();
            _roots.Clear();

            for (var xx = 0; xx < RootsListBox.Items.Count; xx++)
            {
                var root = RootsListBox.Items[xx].ToString();
                _roots.Add(root);
            }

            Application.DoEvents();
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Logger.Verbose("OnRenamed", "Entering::" + e.ToFormattedString());

            var newFile = GetDictionaryKey(e.FullPath);
            var oldFile = GetDictionaryKey(e.OldFullPath);

            var ruleResult = Rules.ProcessPath(oldFile);

            if (ruleResult == RuleResult.Reject)
            {
                Logger.Verbose("OnChanged", oldFile + " failed rules check; rejected");
                return;
            }
            ruleResult = Rules.ProcessPath(newFile);

            if (ruleResult == RuleResult.Reject)
            {
                Logger.Verbose("OnChanged", newFile + " failed rules check; rejected");
                return;
            }

            // Remove old file
            _changeQueue.Queue(new FileChange(ChangeAction.Remove, oldFile, MaxRetries));

            // Add new file
            if (!_replDictionary.ContainsKey(newFile))
            {
                Logger.Verbose("OnRenamed", "newFile (" + newFile + ") not found in _replDictionary, addings");
                _replDictionary.Add(newFile, new ReplInfo(e.FullPath, _roots));
            }

            // Sync File
            _changeQueue.Queue(new FileChange(ChangeAction.Sync, newFile, MaxRetries));
            Logger.Verbose("OnRenamed", "Exiting::" + e.ToFormattedString());

        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Logger.Verbose("OnChanged", "Entered" + e.ToFormattedString());

            var file = GetDictionaryKey(e.FullPath);
            var changeType = e.ChangeType;
            var isDir = false;

            var ruleResult = Rules.ProcessPath(file);

            if (ruleResult == RuleResult.Reject)
            {
                Logger.Verbose("OnChanged", file + " failed rules check; rejected");
                return;
            }

            if (File.Exists(e.FullPath) || Directory.Exists(e.FullPath))
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(e.FullPath);
                    isDir = ((attr & FileAttributes.Directory) == FileAttributes.Directory);
                }
                catch (Exception ex)
                {
                    Logger.Error("Could not get file attributes for " + e.FullPath + "; " + ex.Message);
                }
            }

            Logger.Verbose("OnChanged", string.Format("({3}) {0}{1}; {2}", e.Name, (isDir ? " (directory)" : ""), e.FullPath, e.ChangeType));

            if (isDir)
            {
                //var _dirChanges = DiscoverChangesByDirectory(e.FullPath);

                //if (_dirChanges.Count == 0)
                //{
                return;
                //}
            }

            
            switch (changeType)
            {
                case WatcherChangeTypes.Changed:
                    _changeQueue.Queue(new FileChange(ChangeAction.Sync, file, MaxRetries));
                    break;
                case WatcherChangeTypes.Created:
                    if (!_replDictionary.ContainsKey(file))
                    {
                        _replDictionary.Add(file, new ReplInfo(file, _roots));
                    }
                    _changeQueue.Queue(new FileChange(ChangeAction.Sync, file, MaxRetries));
                    break;
                case WatcherChangeTypes.Deleted:
                    _changeQueue.Queue(new FileChange(ChangeAction.Remove, file, MaxRetries));
                    break;
            }
            Logger.Verbose("OnChanged", "Exiting::file = " + file + "; changeType = " + changeType + "; isDir = " + isDir);
        }

        public Queue<FileChange> DiscoverChangesByDirectory(string fullPath)
        {
            var changes = new Queue<FileChange>();

            // 1. Identify the root

            var dictKey = GetDictionaryKey(fullPath);
            var rootIndex = -1;

            for (var xx = 0; xx < _roots.Count; xx++)
            {
                if (fullPath.StartsWith(_roots[xx]))
                {
                    rootIndex = xx;
                    break;
                }
            }

            if (rootIndex == -1) throw new InvalidOperationException("rootIndex == -1, this should not happen");

            // 2. List all of the files known in that root (they're in the replDict) and get all of the files in file system

            var knownFileKeys = (from k in _replDictionary.Keys
                                 where k.StartsWith(dictKey)
                                 select k).ToList();

            var fsFiles = Directory.GetFiles(fullPath).ToList();

            // 3. Figure out what's missing and create removal changes for them
            for (var xx = 0; xx < knownFileKeys.Count; xx++)
            {
                var path = Path.Combine(_roots[rootIndex], knownFileKeys[xx]);

                if (File.Exists(path)) continue;

                changes.Enqueue(new FileChange(ChangeAction.Remove, knownFileKeys[xx], MaxRetries));
            }

            // 4. Figure out what's new, add them to the replDictionary
            for (var xx = 0; xx < fsFiles.Count; xx++)
            {
                var key = GetDictionaryKey(fsFiles[xx]);

                if (knownFileKeys.Contains(key)) continue;

                if (!_replDictionary.ContainsKey(key))
                {
                    _replDictionary.Add(key, new ReplInfo(fsFiles[xx], _roots));
                }
                changes.Enqueue(new FileChange(ChangeAction.Sync, key, MaxRetries));
            }

            // 5. Return the changes queue 
            return changes;
        }

        private void QueueTimer_Tick(object sender, EventArgs e)
        {
            // Logger.Verbose("QueueTimer_Tick", "Tock?");
            ProcessQueue();
        }


        private void ProcessQueue()
        {
            var tryAgain = new ChangeQueue();
            var maxCount = _changeQueue.Count;
            var startedAt = DateTime.Now;

            _processing = true;

            QueueTimer.Enabled = false;

            while (_changeQueue.Count > 0)
            {
                LabelStatus.Text = DateTime.Now.Subtract(startedAt).ToString("h\\:mm\\:ss");
                var fileChange = _changeQueue.Dequeue();

                if (fileChange == null) break;

                var rulesResult = Rules.ProcessPath(fileChange.File);

                if (rulesResult == RuleResult.Reject)
                {
                    continue;
                }
                if (!_replDictionary.ContainsKey(fileChange.File))
                {
                    Logger.Warning("Could not find key in _replDictionary; " + fileChange.File);
                    if (fileChange.Action != ChangeAction.Remove)
                    {
                        fileChange.RetriesRemaining--;
                        tryAgain.Queue(fileChange);
                    }

                    Application.DoEvents();
                    continue;
                }
                var item = _replDictionary[fileChange.File];
                maxCount = Math.Max(maxCount, _changeQueue.Count);
                PbOpStatus.Minimum = 0;

                try
                {
                    PbOpStatus.Maximum = maxCount;
                    var pbOpStatus = Math.Max(maxCount - _changeQueue.Count, 1);
                    PbOpStatus.Value = pbOpStatus;

                    switch (fileChange.Action)
                    {
                        case ChangeAction.Remove:
                            item.Remove();
                            // _replDictionary.Remove(fileChange.File);
                            break;
                        case ChangeAction.Sync:
                            if (item.Removed)
                            {
                                item.Removed = false;
                            }
                            item.Sync();
                            break;
                    }
                }
                catch (IOException ex)
                {
                    Logger.Error("Could not perform the " + fileChange.Action +
                        " on  " + fileChange.File + " (" +
                        fileChange.RetriesRemaining + "); " + ex.Message);
                    fileChange.RetriesRemaining--;
                    tryAgain.Queue(fileChange);
                }
                Application.DoEvents();
            }

            while (tryAgain.Count > 0)
            {
                _changeQueue.Queue(tryAgain.Dequeue());
            }

            LabelStatus.Text = "";
            QueueTimer.Enabled = true;
            PbOpStatus.Value = 0;

            _processing = false;
            if (!BtnEndSync.Enabled) { BtnStartSync.Enabled = true; }
        }

        private string GetDictionaryKey(string path)
        {
            string root = null;
            for (var xx = 0; xx < _roots.Count; xx++)
            {
                var curRoot = _roots[xx];
                if (path.StartsWith(curRoot))
                {
                    root = curRoot;
                    break;
                }
            }

            if (root == null)
            {
                throw new NullReferenceException("This should never happen. root is null");
            }

            path = path.Substring(root.Length);
            if (path.StartsWith("\\"))
            {
                if (path.Length == 1)
                {
                    throw new InvalidOperationException("path.Length == 1 && path.StartsWith(\"\\\")");
                }

                path = path.Substring(1);
            }

            return path;
        }

        private void LoggerPump_Tick(object sender, EventArgs e)
        {
            while (_loggerPump.Count > 0)
            {
                var message = _loggerPump.Dequeue();
                
                if (message == null) break;

                if (message.TraceLevel > UiTraceLevel) continue;

                var color = Color.Black;

                if (message.TraceLevel == TraceLevel.Verbose) color = Color.DarkSlateGray;
                else if (message.TraceLevel == TraceLevel.Info) color = Color.Black;
                else if (message.TraceLevel == TraceLevel.Warning) color = Color.DarkOrange;
                else if (message.TraceLevel == TraceLevel.Error) color = Color.DarkRed;

                LoggerView.SelectionStart = LoggerView.TextLength;
                LoggerView.SelectionLength = 0;

                LoggerView.SelectionColor = color;
                LoggerView.AppendText(DateTime.Now.ToString("hh\\:mm\\:ss\t"));
                LoggerView.AppendText(message.Message);
                LoggerView.SelectionColor = LoggerView.ForeColor;

                LoggerView.AppendText("\r\n");

                if (LoggerView.Text.Length > MaxOnScreenLogLength)
                {
                    var diff = LoggerView.Text.Length - MaxOnScreenLogLength;
                    var firstCrLf = LoggerView.Text.IndexOf("\r\n", diff);
                    LoggerView.Text = LoggerView.Text.Substring(firstCrLf + 2);
                }

                LoggerView.SelectionStart = LoggerView.Text.Length;
                LoggerView.ScrollToCaret();
            }
            StatustQueueSize.Text = _changeQueue.Count.ToString();

            Application.DoEvents();
        }

        private void BtnEndSync_Click(object sender, EventArgs e)
        {
            QueueTimer.Enabled = false;
            BtnEndSync.Enabled = false;

            while (_processing)
            {
                Logger.Verbose("BtnEndSync_Click", "Waiting for processing queue to empty");
                Application.DoEvents();
            }

            BtnStartSync.Enabled = true;
            Logger.Info("BtnEndSync_Click", "Sync ended");

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CmbLogLevel.SelectedIndex = 1;

            Show();

            if (AutoStart)
            {
                BtnStartSync_Click(this, new EventArgs());
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach(var watcher in _rootWatchers)
            {
                watcher.Dispose();
            }
        }
    }
}
