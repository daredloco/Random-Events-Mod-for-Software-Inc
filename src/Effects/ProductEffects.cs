using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomEvents
{
    public static class ProductEffects
    {
        public static bool TryApply(EventEffect effect)
        {
            SoftwareProduct product = effect.Parent == null ? null : effect.Parent.EventObject as SoftwareProduct;
            int value;
            switch (effect.Effect)
            {
                case "software_offsales":
                    product.AddToCashflow(0, int.Parse(effect.EffectValue), 0, int.Parse(effect.EffectValue) * product.Price, 0, TimeOfDay.Instance.GetDate());
                    return true;
                case "software_onsales":
                    product.AddToCashflow(int.Parse(effect.EffectValue), 0, 0, int.Parse(effect.EffectValue) * product.Price, 0, TimeOfDay.Instance.GetDate());
                    return true;
                case "software_refunds":
                    value = Math.Min(int.Parse(effect.EffectValue), Sales(product, false));
                    product.AddToCashflow(0, 0, value, -product.Price * value, 0, TimeOfDay.Instance.GetDate());
                    return true;
                case "change_bugs":
                    product.AddBugs(int.Parse(effect.EffectValue));
                    return true;
                case "add_marketing":
                    product.AddToMarketing(MarketSimulation.Active.GetMaxAwareness(product) * float.Parse(effect.EffectValue));
                    return true;
            }

            if (effect.Effect == "fans_change")
            {
                if (product != null)
                {
                    GameSettings.Instance.MyCompany.AddFans(int.Parse(effect.EffectValue), product.Category);
                    return true;
                }
                foreach (SoftwareProduct candidate in GameSettings.Instance.MyCompany.Products)
                {
                    GameSettings.Instance.MyCompany.AddFans(int.Parse(effect.EffectValue), candidate.Category);
                    return true;
                }
                return true;
            }

            if (effect.Effect.StartsWith("fans_change_", StringComparison.OrdinalIgnoreCase))
            {
                string[] target = FanTarget(effect.Effect);
                SoftwareProduct candidate = GameSettings.Instance.MyCompany.Products.FirstOrDefault(item => item.Type.Name == target[0] && (target[1] == null || item.Category.Name == target[1]));
                if (candidate != null)
                    GameSettings.Instance.MyCompany.AddFans(int.Parse(effect.EffectValue), candidate.Category);
                return true;
            }

            if (effect.Effect.StartsWith("popularity_change_", StringComparison.OrdinalIgnoreCase))
            {
                if (effect.Effect.Equals("popularity_change_game", StringComparison.OrdinalIgnoreCase))
                {
                    int popularityValue = int.Parse(effect.EffectValue);
                    foreach (var category in MarketSimulation.Active.SoftwareTypes["Game"].Categories)
                        category.Value.Popularity += popularityValue;
                    return true;
                }

                string[] target = PopularityTarget(effect.Effect);
                MarketSimulation.Active.SoftwareTypes[target[0]].Categories[target[1]].Popularity += int.Parse(effect.EffectValue);
                return true;
            }

            return false;
        }

        private static int Sales(SoftwareProduct product, bool online)
        {
            return product.GetUnitSales(online).Sum();
        }

        private static string[] FanTarget(string effect)
        {
            string key = effect.Substring("fans_change_".Length);
            if (key == "os") return new[] { "Operating System", (string)null };
            if (key == "oscomputer") return new[] { "Operating System", "Computer" };
            if (key == "osconsole") return new[] { "Operating System", "Console" };
            if (key == "osphone") return new[] { "Operating System", "Phone" };
            if (key == "office") return new[] { "Office Software", "Default" };
            if (key == "2deditor") return new[] { "2D Editor", "Default" };
            if (key == "3deditor") return new[] { "3D Editor", "Default" };
            if (key == "audiotool") return new[] { "Audio Tool", "Default" };
            if (key == "antivirus") return new[] { "Antivirus", "Default" };
            if (key == "game") return new[] { "Game", null };
            return new[] { "Game", key.Substring("game".Length) == "rpg" ? "RPG" : key.Substring("game".Length) == "adventure" ? "Adventure" : key.Substring("game".Length) == "simulation" ? "Simulation" : key.Substring("game".Length) == "sports" ? "Sports" : key.Substring("game".Length) == "rts" ? "RTS" : "FPS" };
        }

        private static string[] PopularityTarget(string effect)
        {
            string key = effect.Substring("popularity_change_".Length);
            if (key == "oscomputer") return new[] { "Operating System", "Computer" };
            if (key == "osconsole") return new[] { "Operating System", "Console" };
            if (key == "osphone") return new[] { "Operating System", "Phone" };
            if (key == "office") return new[] { "Office Software", "Default" };
            if (key == "2deditor") return new[] { "2D Editor", "Default" };
            if (key == "3deditor") return new[] { "3D Editor", "Default" };
            if (key == "audiotool") return new[] { "Audio Tool", "Default" };
            if (key == "antivirus") return new[] { "Antivirus", "Default" };
            string category = key == "gamerpg" ? "RPG" : key == "gameadventure" ? "Adventure" : key == "gamesimulation" ? "Simulation" : key == "gamesports" ? "Sports" : key == "gamerts" ? "RTS" : "FPS";
            return new[] { "Game", category };
        }
    }
}
