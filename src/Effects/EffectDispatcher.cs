namespace RandomEvents
{
    public static class EffectDispatcher
    {
        public static void Apply(EventEffect effect)
        {
            if (CompanyEffects.TryApply(effect)) return;
            if (ProductEffects.TryApply(effect)) return;
            if (EmployeeEffects.TryApply(effect)) return;
            if (MarketEffects.TryApply(effect)) return;
            MiscEffects.TryApply(effect);
        }
    }
}
