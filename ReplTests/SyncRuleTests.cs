using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Replicator;

namespace ReplTests
{
    [TestClass]
    public class SyncRulesTests
    {
        [TestMethod]
        public void SyncRules_Constructor()
        {
            var rules = new SyncRules();

            Assert.AreEqual(9, rules.Count);
        }

        [TestMethod]
        public void SyncRule_ApplyLeftRightRule_Directory()
        {
            var rules = new SyncRules();
            // c:\users\zooadmin\OneDrive\.dropbox.cache\2018-01-25\AGMSService_20180125 (deleted 2d6f7382ef1401d619e44e64a4a22255).log

            var testData = new[] {
                new { rule = new SyncRule(".", EvaluationType.Directory, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith),
                    path = @"\.dropbox.cache\2018-01-25\AGMSService_20180125 (deleted 2d6f7382ef1401d619e44e64a4a22255).log",
                    dir = @".dropbox.cache\2018-01-25", file = "AGMSService_20180125 (deleted 2d6f7382ef1401d619e44e64a4a22255).log",
                    expected = RuleResult.Reject}
            };

            foreach (var iota in testData)
            {
                var result = rules.ApplyLeftRightRule(iota.rule, iota.path, iota.dir, iota.file);

                Assert.AreEqual(iota.expected, result, "rule: {0}; path: {1}; dir: {2}; file: {3}", iota.rule, iota.path, iota.dir, iota.file);
            }
        }
        [TestMethod]
        public void SyncRule_ApplyLeftRightRule_File()
        {
            var rules = new SyncRules();

            var testData = new[] {
                new { rule = new SyncRule(".", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith),
                    path = "/files/.cache", dir = "files", file = ".cache", expected = RuleResult.Reject},

                new { rule = new SyncRule(".", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith),
                    path = "/files/vs.cache", dir = "files", file = "vs.cache", expected = RuleResult.NotApplicable },

                new { rule = new SyncRule("~", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.EndsWith),
                    path = "/files/file.doc~", dir = "files", file = "file.doc~", expected = RuleResult.Reject},

                new { rule = new SyncRule(".", EvaluationType.File, SyncRuleType.Include, SyncRuleProcessor.StartsWith),
                    path = "/files/.include", dir = "files", file = ".include", expected = RuleResult.Accept }
            };

            foreach (var iota in testData)
            {
                var result = rules.ApplyLeftRightRule(iota.rule, iota.path, iota.dir, iota.file);

                Assert.AreEqual(iota.expected, result, "rule: {0}; path: {1}; dir: {2}; file: {3}", iota.rule, iota.path, iota.dir, iota.file);
            }
        }
    }
}
