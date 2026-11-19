using System;
using System.Collections.Generic;
using System.Linq;
using Tyd;

namespace RandomEvents
{
    public static class MainConditionEvaluator
    {
        public static bool TryEvaluate(string text, out List<object> candidates)
        {
            ConditionExpression expression = ConditionExpression.Parse(text);
            candidates = new List<object>();
            string name = expression.Name;

            if (name == "has_software")
                candidates.AddRange(GameSettings.Instance.MyCompany.Products);
            else if (name == "has_workitem")
                candidates.AddRange(GameSettings.Instance.MyCompany.WorkItems);
            else if (name.StartsWith("has_game", StringComparison.Ordinal))
                AddProducts(candidates, product => product.Type.Name == "Game" && MatchesGameCategory(name, product));
            else if (name == "has_os")
                AddProducts(candidates, product => product.Type.Name == "Operating System");
            else if (name.StartsWith("has_os", StringComparison.Ordinal))
                AddProducts(candidates, product => product.Type.Name == "Operating System" && product.Category.Name == CategoryFor(name));
            else if (name == "has_2deditor" || name == "has_3deditor" || name == "has_audiotool" || name == "has_office" || name == "has_antivirus")
                AddProducts(candidates, product => product.Type.Name == TypeFor(name));
            else if (name == "has_staff")
                candidates.AddRange(GameSettings.Instance.sActorManager.Staff);
            else if (name == "has_employee")
                candidates.AddRange(GameSettings.Instance.sActorManager.Actors.Where(actor => actor.AItype == AI.AIType.Employee && !actor.employee.Founder));
            else if (name.StartsWith("has_", StringComparison.Ordinal) && IsRoleCondition(name))
                candidates.AddRange(GameSettings.Instance.sActorManager.Actors.Where(actor => actor.GetRole() == RoleFor(name)));
            else if (name == "has_stockmarket")
                candidates.AddRange(GameSettings.Instance.StockMarkets);
            else if (name == "has_supportwork")
                candidates.AddRange(GameSettings.Instance.MyCompany.WorkItems.OfType<SupportWork>());
            else if (name == "has_activedeal")
                candidates.AddRange(GameSettings.Instance.MyCompany.WorkItems.Where(item => item.ActiveDeal != null));
            else if (name == "has_softwareworkitem")
                candidates.AddRange(GameSettings.Instance.MyCompany.WorkItems.OfType<SoftwareWorkItem>());
            else if (name == "has_softwarealpha")
                candidates.AddRange(GameSettings.Instance.MyCompany.WorkItems.OfType<SoftwareAlpha>());
            else if (name == "has_designdocument")
                candidates.AddRange(GameSettings.Instance.MyCompany.WorkItems.OfType<DesignDocument>());
            else if (name == "has_multiple_founders")
            {
                int founderCount = GameSettings.Instance.sActorManager.Actors.Count(actor => actor.employee.Founder);
                return expression.Operator == "!" ? founderCount <= 1 : founderCount >= 2;
            }
            else if (name == "has_founder_at_work")
            {
                var founders = GameSettings.Instance.sActorManager.Actors.Where(actor => actor.employee.Founder);
                bool hasFounderAtWork = founders.Any(actor => actor.IsWorking);
                return expression.Operator == "!" ? !hasFounderAtWork : hasFounderAtWork;
            }
            else if (name == "player_has_car")
            {
                Actor founder = GameSettings.Instance.sActorManager.Actors.GetRandomWhere(actor => actor.employee.Founder);
                bool hasCar = founder != null && founder.MyCar != null;
                return expression.Operator == "!" ? !hasCar : hasCar;
            }
            else if (IsDateCondition(name))
                return Compare(name == "is_year" ? TimeOfDay.Instance.GetDate(true).Year : TimeOfDay.Instance.GetDate(true).Month, expression, 1, false);
            else if (name == "has_fans")
                return Compare(GameSettings.Instance.MyCompany.Fans, expression, 1, false);
            else if (name == "has_money")
                return Compare((float)GameSettings.Instance.MyCompany.Money, expression, 1, false);
            else if (name == "has_snow")
                return Compare(TimeOfDay.Instance.SnowAmount, expression, 0.001f, false);
            else if (name == "has_stars")
                return Compare(GameSettings.Instance.MyCompany.BusinessStars, expression, 1, false);
            else if (name == "has_rain")
                return TimeOfDay.Instance.RainFactor >= 0.1f;
            else if (name.StartsWith("has_popularity", StringComparison.Ordinal))
                return Compare(PopularityFor(name), expression, 1, false);
            else
                return true;

            return Compare(candidates.Count, expression, 1, true);
        }

        private static bool Compare(float actual, ConditionExpression expression, float defaultMinimum, bool count)
        {
            if (string.IsNullOrEmpty(expression.Operator))
                return actual >= defaultMinimum;
            float expected = float.Parse(expression.Value);
            switch (expression.Operator)
            {
                case "<": return actual < expected;
                case ">": return actual > expected;
                case "<=": return actual <= expected;
                case ">=": return actual >= expected;
                case "!=": return actual != expected;
                case "=": return actual == expected;
                case "!": return actual <= 0;
                default: return false;
            }
        }

        private static void AddProducts(List<object> result, Func<SoftwareProduct, bool> predicate)
        {
            result.AddRange(GameSettings.Instance.MyCompany.Products.Where(predicate));
        }

        private static bool MatchesGameCategory(string name, SoftwareProduct product)
        {
            if (name == "has_game") return true;
            return product.Category.Name == CategoryFor(name);
        }

        private static string CategoryFor(string name)
        {
            if (name.EndsWith("gamerpg", StringComparison.Ordinal)) return "RPG";
            if (name.EndsWith("gameadventure", StringComparison.Ordinal)) return "Adventure";
            if (name.EndsWith("gamesimulation", StringComparison.Ordinal)) return "Simulation";
            if (name.EndsWith("gamesports", StringComparison.Ordinal)) return "Sports";
            if (name.EndsWith("gamerts", StringComparison.Ordinal)) return "RTS";
            if (name.EndsWith("gamefps", StringComparison.Ordinal)) return "FPS";
            if (name.EndsWith("oscomputer", StringComparison.Ordinal)) return "Computer";
            if (name.EndsWith("osconsole", StringComparison.Ordinal)) return "Console";
            if (name.EndsWith("osphone", StringComparison.Ordinal)) return "Phone";
            return "Default";
        }

        private static string TypeFor(string name)
        {
            if (name == "has_2deditor") return "2D Editor";
            if (name == "has_3deditor") return "3D Editor";
            if (name == "has_audiotool") return "Audio Tool";
            if (name == "has_office") return "Office Software";
            return "Antivirus";
        }

        private static bool IsRoleCondition(string name)
        {
            return name == "has_leader" || name == "has_designer" || name == "has_programmer" || name == "has_artist" || name == "has_service";
        }

        private static Employee.RoleBit RoleFor(string name)
        {
            if (name == "has_leader") return Employee.RoleBit.Lead;
            if (name == "has_designer") return Employee.RoleBit.Designer;
            if (name == "has_programmer") return Employee.RoleBit.Programmer;
            if (name == "has_artist") return Employee.RoleBit.Artist;
            return Employee.RoleBit.Service;
        }

        private static bool IsDateCondition(string name)
        {
            return name == "is_year" || name == "is_month";
        }

        private static float PopularityFor(string name)
        {
            string type = "Game";
            string category = CategoryFor(name.Replace("has_popularity", "has_"));
            if (name.Contains("oscomputer")) { type = "Operating System"; category = "Computer"; }
            else if (name.Contains("osconsole")) { type = "Operating System"; category = "Console"; }
            else if (name.Contains("osphone")) { type = "Operating System"; category = "Phone"; }
            else if (name.Contains("office")) { type = "Office Software"; category = "Default"; }
            else if (name.Contains("2deditor")) { type = "2D Editor"; category = "Default"; }
            else if (name.Contains("3deditor")) { type = "3D Editor"; category = "Default"; }
            else if (name.Contains("audiotool")) { type = "Audio Tool"; category = "Default"; }
            else if (name.Contains("antivirus")) { type = "Antivirus"; category = "Default"; }
            return MarketSimulation.Active.SoftwareTypes[type].Categories[category].Popularity;
        }
    }
}
