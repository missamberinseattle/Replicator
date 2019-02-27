using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Replicator;

namespace ReplTests
{
    [TestClass]
    public class ReplInfoTests
    {
        public const string RootA = "RootA";
        public const string RootB = "RootB";

        public readonly DateTime December2017 = new DateTime(2017, 12, 11, 10, 0, 0);
        public readonly DateTime June2016 = new DateTime(2016, 6, 5, 4, 0, 0);

        public const int IndexRootA = 0;
        public const int IndexRootB = 1;

        public List<string> Roots { get; set; }

        #region Test Initialization
        public void InitializeTest(out string basePath)
        {
            var stack = new StackTrace();
            var frameIndex = 1;

            StackFrame frame = null;
            string methodName = null;

            while (frame == null)
            {
                frame = stack.GetFrame(frameIndex);
                methodName = frame.GetMethod().Name;

                if (methodName.StartsWith("HelperTest_"))
                {
                    frameIndex++;
                    frame = null;
                }
            }

            basePath = Path.Combine(Environment.CurrentDirectory, "TestRoots_" + methodName);

            if (Directory.Exists(basePath))
            {
                RemoveDirectory(basePath);
            }

            Roots = new List<string>();
            Roots.Add(Path.Combine(basePath, RootA));
            Roots.Add(Path.Combine(basePath, RootB));

            Directory.CreateDirectory(Roots[IndexRootA]);
            Directory.CreateDirectory(Roots[IndexRootB]);
        }

        private void RemoveDirectory(string path)
        {
            Directory.Delete(path, true);
            Thread.Sleep(50);

            if (Directory.Exists(path))
            {
                throw new IOException("Directory removal failed; " + path);
            }
        }

        private void CreateFile(string basePath, string filePath, CreateDirective directive)
        {
            var rootAFile = Path.Combine(basePath, RootA, filePath);
            var rootBFile = Path.Combine(basePath, RootB, filePath);

            switch (directive)
            {
                case CreateDirective.OnlyInA:
                    WriteFile(rootAFile, December2017);
                    break;
                case CreateDirective.OnlyInB:
                    WriteFile(rootBFile, December2017);
                    break;
                case CreateDirective.RootANewer:
                    WriteFile(rootAFile, December2017);
                    WriteFile(rootBFile, June2016);
                    break;
                case CreateDirective.RootBNewer:
                    WriteFile(rootAFile, June2016);
                    WriteFile(rootBFile, December2017);
                    break;
                case CreateDirective.SameAge:
                    WriteFile(rootAFile, December2017);
                    WriteFile(rootBFile, December2017);
                    break;
            }
        }

        private void WriteFile(string path, DateTime lastWriteTime)
        {
            using (var text = File.CreateText(path))
            {
                text.Write(path);
                text.Write("\t");
                text.Write(lastWriteTime.ToLongDateString());
                text.Close();
            }

            var info = new FileInfo(path) { LastWriteTime = lastWriteTime };
        }
        #endregion


        #region ReplicationDictionary Tests
        [TestMethod]
        public void BuildReplicationDictionaryAndSync()
        {
            var roots = new List<string>
            {
                @"C:\Users\zooadmin\Dropbox",
                @"C:\Users\zooadmin\OneDrive"
            };
            var rules = new SyncRules();
            var dict = ReplicationDictionary.BuildDictionary(roots, rules);

            foreach (var item in dict)
            {
                var status = item.Value.Sync();
                Logger.Info(status + "; " + item.Key);
            }
        }
        #endregion

        #region Remove Tests
        [TestMethod]
        public void Remove_OnlyInA()
        {
            HelperTest_Remove(CreateDirective.OnlyInA);
        }

        [TestMethod]
        public void Remove_OnlyInB()
        {
            HelperTest_Remove(CreateDirective.OnlyInB);
        }

        [TestMethod]
        public void Remove_NewerInA()
        {
            HelperTest_Remove(CreateDirective.RootANewer);
        }

        [TestMethod]
        public void Remove_NewerInB()
        {
            HelperTest_Remove(CreateDirective.RootBNewer);
        }

        [TestMethod]
        public void RemoveSameAge()
        {
            HelperTest_Remove(CreateDirective.SameAge);
        }

        private void HelperTest_Remove(CreateDirective directive)
        {
            InitializeTest(out string basePath);
            var filePath = "File1.txt";

            CreateFile(basePath, filePath, directive);

            var replInfo = new ReplInfo(filePath, Roots);

            var aPath = Path.Combine(Roots[IndexRootA], filePath);
            var bPath = Path.Combine(Roots[IndexRootB], filePath);

            var status = replInfo.Remove();

            Assert.AreEqual(ReplicationStatus.Removed, status);
            Assert.IsFalse(File.Exists(aPath));
            Assert.IsFalse(File.Exists(bPath));
        }
        #endregion

        #region Sync Tests
        [TestMethod]
        public void Sync_OnlyInA()
        {
            HelperTest_Sync(CreateDirective.OnlyInA);
        }

        [TestMethod]
        public void Sync_OnlyInB()
        {
            HelperTest_Sync(CreateDirective.OnlyInB);
        }

        [TestMethod]
        public void Sync_RootANewer()
        {
            HelperTest_Sync(CreateDirective.RootANewer);
        }

        [TestMethod]
        public void Sync_RootBNewer()
        {
            HelperTest_Sync(CreateDirective.RootBNewer);
        }

        [TestMethod]
        public void Sync_SameAge()
        {
            HelperTest_Sync(CreateDirective.SameAge);
        }


        private void HelperTest_Sync(CreateDirective directive)
        {
            InitializeTest(out string basePath);
            var filePath = "File1.txt";

            CreateFile(basePath, filePath, directive);

            var replInfo = new ReplInfo(filePath, Roots);

            var status = replInfo.Sync();

            var aPath = Path.Combine(Roots[IndexRootA], filePath);
            var bPath = Path.Combine(Roots[IndexRootB], filePath);

            switch (directive)
            {
                case CreateDirective.OnlyInA:
                    Assert.IsTrue(File.Exists(bPath));
                    Assert.AreEqual(December2017, GetLastWriteDate(bPath));
                    Assert.AreEqual(ReplicationStatus.Synced, status);
                    break;
                case CreateDirective.OnlyInB:
                    Assert.IsTrue(File.Exists(aPath));
                    Assert.AreEqual(December2017, GetLastWriteDate(aPath));
                    Assert.AreEqual(ReplicationStatus.Synced, status);
                    break;
                case CreateDirective.RootANewer:
                    Assert.IsTrue(File.Exists(bPath));
                    Assert.AreEqual(December2017, GetLastWriteDate(bPath));
                    Assert.AreEqual(ReplicationStatus.Synced, status);
                    break;
                case CreateDirective.RootBNewer:
                    Assert.IsTrue(File.Exists(aPath));
                    Assert.AreEqual(December2017, GetLastWriteDate(aPath));
                    Assert.AreEqual(ReplicationStatus.Synced, status);
                    break;
                case CreateDirective.SameAge:
                    Assert.AreEqual(ReplicationStatus.Synced, status);
                    break;
            }
        }

        private DateTime GetLastWriteDate(string path)
        {
            var fileInfo = new FileInfo(path);
            return fileInfo.LastWriteTime;
        }
        #endregion

        #region GetFullPath Tests
        [TestMethod]
        public void GetFullPath_CompletedSync()
        {
            InitializeTest(out string basePath);
            var filePath = "File1.txt";
            var expectedPath = Path.Combine(Roots[IndexRootA], filePath);

            CreateFile(basePath, filePath, CreateDirective.OnlyInA);

            var replInfo = new ReplInfo(filePath, Roots);

            var path = replInfo.GetFullPath(IndexRootA);

            Assert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        public void GetFullPath_ImcompleteSync()
        {
            InitializeTest(out string basePath);
            var filePath = "File1.txt";
            var expectedPath = Path.Combine(Roots[IndexRootB], filePath);

            CreateFile(basePath, filePath, CreateDirective.OnlyInA);

            var replInfo = new ReplInfo(filePath, Roots);

            var path = replInfo.GetFullPath(IndexRootB);

            Assert.AreEqual(expectedPath, path);
        }
        #endregion

        #region Status Tests
        [TestMethod]
        public void Status_OnlyInA()
        {
            HelperTest_Status(CreateDirective.OnlyInA);
        }

        [TestMethod]
        public void Status_OnlyInB()
        {
            HelperTest_Status(CreateDirective.OnlyInB);
        }

        [TestMethod]
        public void Status_RootANewer()
        {
            HelperTest_Status(CreateDirective.RootANewer);
        }

        [TestMethod]
        public void Status_RootBNewer()
        {
            HelperTest_Status(CreateDirective.RootBNewer);
        }

        [TestMethod]
        public void Status_SameAge()
        {
            HelperTest_Status(CreateDirective.SameAge);
        }

        private void HelperTest_Status(CreateDirective directive)
        {
            InitializeTest(out string basePath);
            var filePath = "File1.txt";

            CreateFile(basePath, filePath, directive);

            var replInfo = new ReplInfo(filePath, Roots);

            var status = replInfo.Status();

            if (directive == CreateDirective.OnlyInA || directive == CreateDirective.OnlyInB)
            {
                // File does not exist across all roots
                Assert.AreEqual(ReplicationStatus.IncompleteReplication, status);
            }
            else if (directive == CreateDirective.RootANewer || directive == CreateDirective.RootBNewer)
            {
                // One file n the roots are is more recent
                Assert.AreEqual(ReplicationStatus.WriteDateMismatch, status);
            }
            else if (directive == CreateDirective.SameAge)
            {
                // Replication has completed and the files are the same age
                Assert.AreEqual(ReplicationStatus.Synced, status);
            }
        }
        #endregion

        #region GetFileDictionary Tests
        [TestMethod]
        public void GetFileDictionary_OnlyInA()
        {
            HelperTest_GetFileDictionary(CreateDirective.OnlyInA);
        }

        [TestMethod]
        public void GetFileDictionary_OnlyInB()
        {
            HelperTest_GetFileDictionary(CreateDirective.OnlyInB);
        }

        [TestMethod]
        public void GetFileDictionary_RootANewer()
        {
            HelperTest_GetFileDictionary(CreateDirective.RootANewer);
        }

        [TestMethod]
        public void GetFileDictionary_RootBNewer()
        {
            HelperTest_GetFileDictionary(CreateDirective.RootBNewer);
        }

        [TestMethod]
        public void GetFileDictionary_SameAge()
        {
            HelperTest_GetFileDictionary(CreateDirective.SameAge);
        }

        private void HelperTest_GetFileDictionary(CreateDirective directive)
        {
            InitializeTest(out string basePath);
            var filePath = "File1.txt";

            CreateFile(basePath, filePath, directive);

            var replInfo = new ReplInfo(filePath, Roots);

            var files = replInfo.GetFileDictionary();

            if (directive != CreateDirective.OnlyInB)
            {
                Assert.IsNotNull(files[IndexRootA]);
                Assert.AreEqual(files[IndexRootA].Name, filePath);
            }

            if (directive != CreateDirective.OnlyInA)
            {
                Assert.IsNotNull(files[IndexRootB]);
                Assert.AreEqual(files[IndexRootB].Name, filePath);
            }
        }
        #endregion

        #region Constructor Tests
        [TestMethod]
        public void Constructor_OnlyInA()
        {
            HelperTest_Constructor(CreateDirective.OnlyInA);

        }

        [TestMethod]
        public void Constructor_OnlyInB()
        {
            HelperTest_Constructor(CreateDirective.OnlyInB);
        }

        [TestMethod]
        public void Constructor_RootANewer()
        {
            HelperTest_Constructor(CreateDirective.RootANewer);
        }

        [TestMethod]
        public void Constructor_RootBNewer()
        {
            HelperTest_Constructor(CreateDirective.RootBNewer);
        }

        [TestMethod]
        public void Constructor_SameAge()
        {
            HelperTest_Constructor(CreateDirective.SameAge);
        }

        private void HelperTest_Constructor(CreateDirective directive)
        {
            InitializeTest(out string basePath);
            var filePath = "File1.txt";

            CreateFile(basePath, filePath, directive);

            var replInfo = new ReplInfo(filePath, Roots);

            // Assert all roots are present
            for (var rootId = 0; rootId < replInfo.Roots.Count; rootId++)
            {
                Assert.AreEqual(Roots[rootId], replInfo.Roots[rootId]);
            }

            // Assert that the Removed flag is false
            Assert.IsFalse(replInfo.Removed);
        }
        #endregion

    }
}
