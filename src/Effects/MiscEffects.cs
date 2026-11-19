using System.Linq;

namespace RandomEvents
{
    public static class MiscEffects
    {
        public static bool TryApply(EventEffect effect)
        {
            if (effect.Effect != "call_event")
                return false;

            Event eventToCall = EventsHandlerV2.RandomEvents.FirstOrDefault(item => item.Title == effect.EffectValue);
            if (eventToCall != null)
                eventToCall.Fire(false);
            return true;
        }
    }
}
