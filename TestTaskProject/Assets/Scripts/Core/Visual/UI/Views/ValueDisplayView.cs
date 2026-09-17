using UnityEngine;

namespace Core.Visual.UI
{
    public abstract class ValueDisplayView<TValue> : MonoBehaviour
    {
        public abstract void DisplayValue(TValue value);
        
        public abstract TValue Displayed { get; }
    }

    public abstract class FloatDisplayView : ValueDisplayView<float> { }
    public abstract class StringDisplayView : ValueDisplayView<string> { }
}