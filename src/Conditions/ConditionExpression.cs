using System;

namespace RandomEvents
{
    public class ConditionExpression
    {
        public string Name { get; private set; }
        public string Operator { get; private set; }
        public string Value { get; private set; }

        private ConditionExpression(string name, string @operator, string value)
        {
            Name = name;
            Operator = @operator;
            Value = value;
        }

        public static ConditionExpression Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new ConditionExpression(string.Empty, string.Empty, string.Empty);

            string[] operators = { "<=", ">=", "!=", "<", ">", "=", "!" };
            foreach (string @operator in operators)
            {
                int index = text.IndexOf(@operator, StringComparison.Ordinal);
                if (index >= 0)
                {
                    return new ConditionExpression(
                        text.Substring(0, index),
                        @operator,
                        text.Substring(index + @operator.Length));
                }
            }

            return new ConditionExpression(text, string.Empty, string.Empty);
        }
    }
}
