using System;
using System.Collections.Generic;

namespace RandomEvents
{
    public static class CompanyEffects
    {
        private static readonly Dictionary<string, Action<EventEffect>> Handlers =
            new Dictionary<string, Action<EventEffect>>(StringComparer.OrdinalIgnoreCase)
            {
                { "company_money", effect => GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.NA) },
                { "company_bill", effect => GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.Bills) },
                { "company_deals", effect => GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.Deals) },
                { "pay_debt", effect => GameSettings.Instance.MyCompany.MakeTransaction((float)-GameSettings.Instance.MyCompany.Money, Company.TransactionCategory.NA) },
                { "change_businessrep", effect => GameSettings.Instance.MyCompany.ChangeBusinessRep(int.Parse(effect.EffectValue), "Event") }
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
