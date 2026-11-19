using UnityEngine;

namespace RandomEvents
{
    public class EventEffect
    {
        public string Effect { get; set; }
        public string EffectName { get; set; }
        public float EffectMinValue { get; set; }
        public float EffectMaxValue { get; set; }
        public string EffectValue { get; set; }
        public bool IsVisible { get; set; }
        internal Event Parent { get; private set; }

        public void SetParent(Event parent)
        {
            Parent = parent;
        }

        public void GenerateValue()
        {
            if (EffectMaxValue != 0)
                EffectValue = ((int)UnityEngine.Random.Range(EffectMinValue, EffectMaxValue)).ToString();
        }

        public string Fire()
        {
            EffectDispatcher.Apply(this);
            return EffectValue;
        }
    }
}
