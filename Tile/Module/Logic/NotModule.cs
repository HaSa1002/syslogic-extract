using UnityEngine.UIElements;

namespace Microlayer
{
    public class NotModule : Logic
    {


        private readonly MicroData[] _values = { 0.5f };
        private bool _firstUiRun = true;

        public override void Execute()
        {
            if (Input[0] >= _values[0])
            {
                PushData(MicroData.Low);
                UpdateEmissionValue(0);
            }
            if (Input[0] < _values[0])
            {
                PushData(MicroData.High);
                UpdateEmissionValue(1);
            }
        }

        public override void ShowUi()
        {
            base.ShowUi();
            if (!_firstUiRun) return;

            var slider = ModuleInfo.rootVisualElement.Q<Slider>();
            slider.value = (float)_values[0];
            slider.RegisterValueChangedCallback(evt => _values[0] = evt.newValue);
            _firstUiRun = false;
        }
    }
}