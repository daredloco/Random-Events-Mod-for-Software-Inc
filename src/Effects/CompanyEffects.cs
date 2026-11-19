namespace RandomEvents
{
    public static class CompanyEffects
    {
        public static bool Apply(EventEffect effect)
        {
            if (effect.Effect == "company_money")
            {
                GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.NA);
                return true;
            }
            if (effect.Effect == "company_bill")
            {
                GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.Bills);
                return true;
            }
            if (effect.Effect == "company_deals")
            {
                GameSettings.Instance.MyCompany.MakeTransaction(float.Parse(effect.EffectValue), Company.TransactionCategory.Deals);
                return true;
            }
            if (effect.Effect == "pay_debt")
            {
                GameSettings.Instance.MyCompany.MakeTransaction((float)-GameSettings.Instance.MyCompany.Money, Company.TransactionCategory.NA);
                return true;
            }
            if (effect.Effect == "change_businessrep")
            {
                GameSettings.Instance.MyCompany.ChangeBusinessRep(int.Parse(effect.EffectValue), "Event");
                return true;
            }
            return false;
        }
    }
}
