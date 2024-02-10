using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microlayer
{
    public class Buffer : Logic
    {
        private readonly MicroData[] _values = { 0.5f };
        private bool _firstUiRun = true;
        private bool _firstExecute = true;
        private float _timer;
        private float _timeStamp;
        private Queue<float> _buffer = new (new []{ 0f });

        public float BufferTime = 2f;

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            if (_firstExecute)
            {
                _timeStamp = Time.time;
                _firstExecute = false;
            }

            var output = new List<MicroData>() { _buffer.Peek() };
            PushData(output);
            UpdateEmissionValue(output);

            if (_timer > (float)_values[0] * BufferTime)
            {
                _buffer.Enqueue((float)Input[0]);
                _buffer.Dequeue();
                return;
            }

            _buffer.Enqueue(0f);
            var time = Time.time;
            _timer = time - _timeStamp;
        }

        public override void ShowUi()
        {
            base.ShowUi();
            if (!_firstUiRun) return;

            var slider = ModuleInfo.rootVisualElement.Q<Slider>();
            slider.value = (float)_values[0];
            slider.RegisterValueChangedCallback(evt =>
            {
                _values[0] = evt.newValue;
                _timer = 0f;
                _firstExecute = true;
                _buffer = new Queue<float>(new []{ 0f });
            });
            _firstUiRun = false;
        }
    }
}
