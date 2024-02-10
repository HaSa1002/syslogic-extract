using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Microlayer
{
    public class Oscillator : Logic
    {
        private static float MaxFrequency = 10f;
        private readonly MicroData[] _frequency = { true };
        //later enum dropdown for sqr, sine, rnd
        private bool _firstUiRun = true;

        public override void Execute()
        {
            UpdateEmissionValue(Input[0]);

            if (Input[0] > 0f && _frequency[0] > 0f)
            {
                PushData(new MicroData[] { ((float)Input[0] / 2f) + (Mathf.Sin(Time.time * (float)_frequency[0] * (float)_frequency[0] * MaxFrequency)) * ((float)Input[0]/2f) }); //sqr
                return;
            }
            PushData(MicroData.Low);
        }

        public override bool IsExecutionSource()
        {
            return true;
        }

        public override void ShowUi()
        {
            base.ShowUi();
            if (!_firstUiRun) return;

            var slider = ModuleInfo.rootVisualElement.Q<Slider>();
            slider.value = (float)_frequency[0];
            slider.RegisterValueChangedCallback(evt => _frequency[0] = evt.newValue);
            _firstUiRun = false;
        }
    }
}
