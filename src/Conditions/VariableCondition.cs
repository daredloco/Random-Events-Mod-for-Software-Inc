using System;
using System.Collections.Generic;
using static RandomEvents.EventsHandlerV2;

namespace RandomEvents
{
    public class VariableCondition
    {
        public object ConditionObject { get; set; }
        public string Main { get; private set; }
        public string[] Subs { get; private set; }

        public VariableCondition(string text)
        {
            string[] parts = (text ?? string.Empty).Split('&');
            Main = parts.Length == 0 ? string.Empty : parts[0];
            Subs = new string[Math.Max(0, parts.Length - 1)];
            Array.Copy(parts, 1, Subs, 0, Subs.Length);
        }

        public bool Check()
        {
            if (debugMode)
                DevConsole.Console.Log("Checking Condition " + Main);

            ConditionObject = null;
            List<object> candidates;
            if (!MainConditionEvaluator.TryEvaluate(Main, out candidates))
                return false;

            foreach (string sub in Subs)
            {
                if (debugMode)
                    DevConsole.Console.Log("Checking Sub Condition " + sub);
                candidates = SubConditionEvaluator.Filter(candidates, sub);
                if (candidates.Count == 0)
                    return false;
            }

            if (candidates.Count > 0)
                ConditionObject = candidates.GetRandom();
            return true;
        }
    }
}
