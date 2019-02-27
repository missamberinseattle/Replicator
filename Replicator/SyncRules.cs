using System;
using System.Collections.Generic;
using System.IO;

namespace Replicator
{
    public class SyncRules : List<SyncRule>
    {
        #region Constructors, Destructors, and Initializers
        public SyncRules()
        {
            AddDefaultRules();
        }

        public SyncRules(int capacity) : base(capacity)
        {
            AddDefaultRules();
        }

        public SyncRules(IEnumerable<SyncRule> collection) : base(collection)
        {
            AddDefaultRules();
        }

        private void AddDefaultRules()
        {
            Add(new SyncRule(".", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith));
            Add(new SyncRule(".", EvaluationType.Directory, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith));
            Add(new SyncRule(".msg", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.EndsWith));
            Add(new SyncRule(".mov", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.EndsWith));
            Add(new SyncRule(".mts", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.EndsWith));
            Add(new SyncRule(".m2ts", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.EndsWith));
            Add(new SyncRule(".avi", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.EndsWith));
            Add(new SyncRule(".dropbox", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.EndsWith));
            Add(new SyncRule("~", EvaluationType.File, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith ));
            Add(new SyncRule("2016 BurlyCon", EvaluationType.Directory, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith));
            Add(new SyncRule("2017 BurlyCon", EvaluationType.Directory, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith));
            Add(new SyncRule("Inspiration", EvaluationType.Directory, SyncRuleType.Exclude, SyncRuleProcessor.StartsWith));
        }
        #endregion

        #region Public Methods
        public RuleResult ProcessPath(string path)
        {
            var file = Path.GetFileName(path);
            var dir = Path.GetDirectoryName(path);

            for (var xx = 0; xx < Count; xx++)
            {
                var rule = this[xx];
                RuleResult result = RuleResult.NotApplicable;

                switch (rule.Processor)
                {
                    case SyncRuleProcessor.StartsWith:
                        result = ApplyLeftRightRule(rule, path, dir, file);
                        break;
                    case SyncRuleProcessor.EndsWith:
                        result = ApplyLeftRightRule(rule, path, dir, file);
                        break;
                    case SyncRuleProcessor.RegEx:
                        throw new NotImplementedException();
                }

                if (result == RuleResult.Reject || result == RuleResult.Accept) return result;
            }

            return RuleResult.Accept;
        }

        public RuleResult ApplyLeftRightRule(SyncRule rule, string path, string dir, string file)
        {
            string examinationTarget = null;
            string logFormat = "{0} matches {1} {5} rule \"{2}\" {3}, returning {4}";
            var result = RuleResult.NotApplicable;


            switch (rule.Evaluation)
            {
                case EvaluationType.File:
                    examinationTarget = file;
                    break;
                case EvaluationType.Directory:
                    examinationTarget = dir;
                    break;
                case EvaluationType.FullPath:
                    throw new NotSupportedException("StartWith rules are not supportedin conjuction with FullPath evaluations");
            }

            var segments = examinationTarget.Split(new[] { '\\' });

            foreach (var segment in segments)
            {
                var ruleMatch = (rule.Processor == SyncRuleProcessor.StartsWith &&
                    segment.StartsWith(rule.Rule, StringComparison.InvariantCultureIgnoreCase)) ||
                    (rule.Processor == SyncRuleProcessor.EndsWith &&
                    segment.EndsWith(rule.Rule, StringComparison.InvariantCultureIgnoreCase));

                if (rule.RuleType == SyncRuleType.Include && ruleMatch == true)
                {
                    result = RuleResult.Accept;
                    break;
                }
                else if (rule.RuleType == SyncRuleType.Exclude && ruleMatch == true)
                {
                    result = RuleResult.Reject;
                    break;
                }
                if (ruleMatch)
                {
                    Logger.Info(string.Format(logFormat, examinationTarget, rule.Evaluation, rule.Rule, rule.RuleType, result, rule.Processor));
                }
            }

            return result;
        }
        #region Overrides
        
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
        #endregion
        #endregion
    }

}