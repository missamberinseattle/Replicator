using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Replicator
{
    public class ReplInfo
    {
        // 1. Add file information (create catalog item)
        // 2. Synchronize files
        //    A. File exists in root1, but not root2 = Copy to root2
        //    B. File exists in root2, but not root1 = Copy to root1
        //    C. File exists in both, root1 is more recent = copy from root1 to root2
        //    D. File exists in both, root2 is more recent = copy from root2 to root1
        // 3. Remove file

        public ReplInfo(string path, List<string> roots)
        {
            Roots = roots;
            FilePath = path;
        }

        public List<string> Roots { get; private set; }
        public bool Removed { get;  set; }
        public string FilePath { get; private set; }

        private Dictionary<int, FileInfo> _itemInfo = new Dictionary<int, FileInfo>();
        
        public ReplicationStatus Remove()
        {
            var status = ReplicationStatus.Removed;
            var rootCount = 0;

            for (var idx = 0; idx < Roots.Count; idx++)
            {
                var path = GetFullPath(idx);

                if (!File.Exists(path))
                {
                    Logger.Verbose("ReplInfo.Remove", "File not found in root; " + path);
                    rootCount++;
                    continue;
                }

                try
                {
                    File.Delete(path);
                    Removed = true;
                    rootCount++;
                }
                catch (Exception ex)
                {
                    Logger.Warning("ReplInfo.Remove", "Could not remove file; " + path);
                    Logger.Warning("ReplInfo.Remove", ex.Message);
                }

                if (File.Exists(path))
                {
                    status = ReplicationStatus.IncompleteReplication;
                    rootCount--;
                }
            }

            if (rootCount == Roots.Count)
            {
                Logger.Info("ReplInfo.Remove", "Removed: " + FilePath);
            }

            return status;
        }

        public ReplicationStatus Sync()
        {
            Logger.Verbose("ReplInfo.Sync", "Syncing " + FilePath);
            if (Removed)
            {
                Logger.Info("ReplInfo.Sync", "Removed, skipped sync. " + FilePath);
                return ReplicationStatus.Skipped;
            }

            var files = GetFileDictionary();

            var dateSorted = (from d in files
                          where d.Value != null
                          orderby d.Value.LastWriteTime descending
                          select d.Key).ToList();

            if (dateSorted.Count == 0)
            {
                Logger.Info("ReplInfo.Sync", "No files to replicate");
                return ReplicationStatus.NoFiles;
            }
            
            var newest = dateSorted[0];
            var oldest = dateSorted[dateSorted.Count - 1];

            if (dateSorted.Count > 1)
            {
                if (files[dateSorted[0]].LastWriteTime == files[dateSorted[dateSorted.Count-1]].LastWriteTime)
                {
                    Logger.Verbose("ReplInfo.Sync", this.FilePath + "; All files with the same time stamp " + 
                        "(" + files[dateSorted[dateSorted.Count - 1]].LastWriteTime.ToString() + ")");
                    return ReplicationStatus.Synced;
                }
            }
            var date = files[newest].LastWriteTime;
            var srcPath = GetFullPath(newest);
            var errorRaised = false;

            for (var index = 0; index < Roots.Count; index++)
            {
                if (index == newest)
                {
                    Logger.Verbose("ReplInfo.Sync", "Newest version of file dated " + files[index].LastWriteTime);
                    continue;
                }
                var dstPath = GetFullPath(index);

                try
                {
                    var destDir = Directory.GetParent(dstPath);
                    if (!destDir.Exists)
                    {
                        Directory.CreateDirectory(destDir.FullName);
                    }

                    File.Copy(srcPath, dstPath, true);
                    Logger.Verbose("ReplInfo.Sync", "Copied to " + dstPath);
                }
                catch (Exception ex)
                {
                    Logger.Error("ReplInfo.Sync", "Could not copy file to " + dstPath + "; " + ex.Message);
                    errorRaised = true;
                }

                try
                {
                    var fileInfo = new FileInfo(dstPath);
                    fileInfo.LastWriteTime = date;
                }
                catch (Exception ex)
                {
                    Logger.Error("ReplInfo.Sync", "Could not set LastWrite on " + 
                        dstPath + "; " + ex.Message);
                    errorRaised = true;
                }
            }

            if (!errorRaised)
            {
                Logger.Info("ReplInfo.Sync", "Sync completed for " + FilePath);
            }
            else
            {
                Logger.Info("ReplInfo.Sync", "Sync completed with errors for " + FilePath);
            }

            return ReplicationStatus.Synced;
        }
        public ReplicationStatus Status()
        {
            if (Removed) return ReplicationStatus.Removed;

            var files = GetFileDictionary();

            var modDates = (from d in files.Values
                            where d != null
                            orderby d.LastWriteTime
                            select d.LastWriteTime).ToList();

            if (modDates.Count != Roots.Count)
            {
                return ReplicationStatus.IncompleteReplication;
            }

            if (modDates[0] != modDates[modDates.Count - 1])
            {
                return ReplicationStatus.WriteDateMismatch;
            }
            
            return ReplicationStatus.Synced;
        }

        public Dictionary<int, FileInfo> GetFileDictionary()
        {
            var fileInfos = new Dictionary<int, FileInfo>();

            for(var xx = 0; xx < Roots.Count; xx++)
            {
                var path = GetFullPath(xx);
                if (!File.Exists(path))
                {
                    fileInfos.Add(xx, null);
                }
                else
                {
                    var fileInfo = new FileInfo(path);
                    fileInfos.Add(xx, fileInfo);
                }
            }

            return fileInfos;
        }

        public string GetFullPath(int rootId)
        {
            return Path.Combine(Roots[rootId], FilePath);
        }
    }
}
