using UnityEngine.UIElements;
using UnityEngine;

namespace Microlayer
{
    public class Constant : ExecutableModule
    {
        private readonly MicroData[] _values = { true };
        private bool _firstUiRun = true;

        private Animator _anim;
        private static readonly int ConstParam = Animator.StringToHash("Const_Param");

        protected override void Start()
        {
            base.Start();

            _anim = GetComponentInChildren<Animator>();
            _anim.SetFloat(ConstParam, 1.0f);
        }

        public override void Execute()
        {
            PushData(_values);
            UpdateEmissionValue(_values[0]);
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
            slider.value = (float)_values[0];
            slider.RegisterValueChangedCallback(evt => _values[0] = evt.newValue);
            slider.RegisterValueChangedCallback(_ => OnChangeValue());
            _firstUiRun = false;
        }

        void OnChangeValue()
        {
            _anim.SetFloat(ConstParam, (float)_values[0]);
        }
    }
}
