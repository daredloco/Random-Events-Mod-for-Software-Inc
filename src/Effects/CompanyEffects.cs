using System;
using System.Collections.Generic;

namespace RandomEvents
{
    internal static class CompanyEffects
    {
        private static readonly Dictionary<string, Action<EventEffect>> Handlers =
            new Dictionary<string, Action<EventEffect>>(StringComparer.OrdinalIgnoreCase)
            {
                { "company_money", effect => GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.NA) },
                { "company_bill", effect => GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.Bills) },
                { "company_deals", effect => GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.Deals) }
            };

        public static bool TryApply(EventEffect effect)
        {
            Action<EventEffect> handler;
            if (!Handlers.TryGetValue(effect.Effect, out handler))
                return false;

            handler(effect);
            return true;
        }
    }
}
