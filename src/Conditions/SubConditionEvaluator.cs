using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomEvents
{
    public static class SubConditionEvaluator
    {
        public static List<object> Filter(List<object> source, string text)
        {
            ConditionExpression expression = ConditionExpression.Parse(text);
            string name = expression.Name;
            if (name == "has_sequel") return FilterProducts(source, product => expression.Operator == "!" ? !product.HasSequel : product.HasSequel);
            if (name == "is_sequel") return FilterProducts(source, product => expression.Operator == "!" ? product.SequelTo == null : product.SequelTo != null);
            if (name == "is_alpha") return FilterAlphas(source, alpha => expression.Operator == "!" ? alpha.InBeta : !alpha.InBeta);
            if (name == "is_beta") return FilterAlphas(source, alpha => expression.Operator == "!" ? !alpha.InBeta : alpha.InBeta);
            if (name == "from_playercompany") return FilterCompany(source, expression.Operator != "!");
            if (name == "has_addon") return FilterProducts(source, product => product.Addons.Count > 0);
            if (name == "release_date") return FilterProducts(source, product => Compare(product.Release, Utils.RemoveDateTime(TimeOfDay.Instance.GetDate(true), int.Parse(expression.Value)), expression.Operator));
            if (name == "has_bugs") return FilterProducts(source, product => Compare(product.Bugss, int.Parse(expression.Value), expression.Operator));
            if (name == "has_offsales") return FilterProducts(source, product => Compare(Sales(product, false), int.Parse(expression.Value), expression.Operator));
            if (name == "has_onsales") return FilterProducts(source, product => Compare(Sales(product, true), int.Parse(expression.Value), expression.Operator));
            if (name == "sick_days") return FilterActors(source, actor => Compare(actor.SickDays, int.Parse(expression.Value), expression.Operator));
            if (name == "is_female") return FilterActors(source, actor => actor.employee.Female == (expression.Operator != "!"));
            if (name == "is_working") return FilterActors(source, actor => actor.IsWorking == (expression.Operator != "!"));
            if (name == "is_founder") return FilterActors(source, actor => actor.employee.Founder == (expression.Operator != "!"));
            if (name == "is_crunching") return FilterActors(source, actor => actor.IsCrunching() == (expression.Operator != "!"));
            if (name == "years_in_company") return new List<object>();
            if (name == "is_stressed") return FilterActors(source, actor => Compare(actor.employee.Stress, float.Parse(expression.Value), expression.Operator));
            if (name == "is_satisfied") return FilterActors(source, actor => Compare(actor.employee.JobSatisfaction, float.Parse(expression.Value), expression.Operator));
            if (name == "salary") return FilterActors(source, actor => Compare(actor.GetMonthlySalary(), float.Parse(expression.Value), expression.Operator));
            return source;
        }

        private static List<object> FilterProducts(IEnumerable<object> source, Func<SoftwareProduct, bool> predicate)
        {
            return source.OfType<SoftwareProduct>().Where(predicate).Cast<object>().ToList();
        }

        private static List<object> FilterActors(IEnumerable<object> source, Func<Actor, bool> predicate)
        {
            return source.OfType<Actor>().Where(predicate).Cast<object>().ToList();
        }

        private static List<object> FilterAlphas(IEnumerable<object> source, Func<SoftwareAlpha, bool> predicate)
        {
            return source.OfType<SoftwareAlpha>().Where(predicate).Cast<object>().ToList();
        }

        private static List<object> FilterCompany(IEnumerable<object> source, bool ownCompany)
        {
            return source.Where(value =>
            {
                DesignDocument design = value as DesignDocument;
                SoftwareAlpha alpha = value as SoftwareAlpha;
                Company company = design != null ? design.MyCompany : alpha != null ? alpha.MyCompany : null;
                return company != null && (company == GameSettings.Instance.MyCompany) == ownCompany;
            }).ToList();
        }

        private static int Sales(SoftwareProduct product, bool online)
        {
            return product.GetUnitSales(online).Sum();
        }

        private static bool Compare(float actual, float expected, string @operator)
        {
            switch (@operator)
            {
                case "<": return actual < expected;
                case ">": return actual > expected;
                case "<=": return actual <= expected;
                case ">=": return actual >= expected;
                case "!=": return actual != expected;
                case "=": return actual == expected;
                default: return actual > 0;
            }
        }

        private static bool Compare(SDateTime actual, SDateTime expected, string @operator)
        {
            switch (@operator)
            {
                case "<": return actual < expected;
                case ">": return actual > expected;
                case "<=": return actual <= expected;
                case ">=": return actual >= expected;
                case "!=": return actual != expected;
                case "=": return actual == expected;
                default: return false;
            }
        }
    }
}
