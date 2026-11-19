using System;
using UnityEngine;

namespace RandomEvents
{
    public static class MarketEffects
    {
        public static bool TryApply(EventEffect effect)
        {
            if (effect.Effect == "stockmarket_loss" || effect.Effect == "stockmarket_win")
            {
                bool loss = effect.Effect.EndsWith("loss", StringComparison.OrdinalIgnoreCase);
                foreach (StockMarket market in GameSettings.Instance.StockMarkets)
                    market.Value = Utils.GetPercentage(market.Value, int.Parse(effect.EffectValue), loss);
                return true;
            }

            if (effect.Effect == "stockmarket_singleloss" || effect.Effect == "stockmarket_singlewin")
            {
                StockMarket market = effect.Parent == null ? null : effect.Parent.EventObject as StockMarket;
                if (market == null && GameSettings.Instance.StockMarkets.Count > 0)
                    market = GameSettings.Instance.StockMarkets[UnityEngine.Random.Range(0, GameSettings.Instance.StockMarkets.Count)];
                if (market != null)
                    market.Value = Utils.GetPercentage(market.Value, int.Parse(effect.EffectValue), effect.Effect.EndsWith("loss", StringComparison.OrdinalIgnoreCase));
                return true;
            }
            return false;
        }
    }
}
