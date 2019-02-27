using System.Text;

namespace Replicator
{
    public class SyncRule
    {
        public SyncRule(string rule, EvaluationType evaluation, SyncRuleType ruleType, SyncRuleProcessor processor)
        {
            Rule = rule;
            Evaluation = evaluation;
            RuleType = ruleType;
            Processor = processor;
        }

        public EvaluationType Evaluation { get; set; }
        public SyncRuleType RuleType { get; set; }
        public SyncRuleProcessor Processor { get; set; }
        public string Rule { get; set; }

        public override string ToString()
        {
            var output = new StringBuilder();

            output.Append("{");
            output.AppendFormat(" {0}; {1}; {2}; {3} ", Rule, Evaluation, RuleType, Processor);
            output.Append("}");

            return output.ToString();
        }
    }

    public enum EvaluationType
    {
        Directory,
        File,
        FullPath
    }

    public enum SyncRuleProcessor
    {
        RegEx,
        StartsWith,
        EndsWith
    }

    public enum SyncRuleType
    {
        Include,
        Exclude
    }

    public enum RuleResult
    {
        Accept,
        Reject,
        NotApplicable
    }
}