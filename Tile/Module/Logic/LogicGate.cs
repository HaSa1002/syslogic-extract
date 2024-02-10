using UnityEngine.UIElements;
using TMPro;

namespace Microlayer
{
    public class LogicGate : Logic
    {
        public enum Type
        {
            And,
            Or,
            Xor,
        }

        public Type ComparisonType = Type.And;
        private bool _firstUiRun = true;
        private TextMeshPro _textMesh;

        protected override void Start()
        {
            base.Start();
            _textMesh = GetComponentInChildren<TextMeshPro>();
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            switch (ComparisonType)
            {
                case Type.And:
                    And();
                    break;
                case Type.Or:
                    Or();
                    break;
                case Type.Xor:
                    Xor();
                    break;
            }
        }

        private void And()
        {
            if (Input[0] && Input[1])
            {
                PushData(MicroData.High);
                UpdateEmissionValue(MicroData.High);
            }
            else
            {
                PushData(MicroData.Low);
                UpdateEmissionValue(MicroData.Low);
            }
        }

        private void Or()
        {
            if (Input[0] || Input[1])
            {
                PushData(MicroData.High);
            }
            else
            {
                PushData(MicroData.Low);
            }
        }

        private void Xor()
        {
            if (Input[0] ^ Input[1])
            {
                PushData(MicroData.High);
            }
            else
            {
                PushData(MicroData.Low);
            }
        }

        public override void ShowUi()
        {
            base.ShowUi();
            if (!_firstUiRun) return;
            _firstUiRun = false;
            var field = ModuleInfo.rootVisualElement.Q<DropdownField>();
            field.RegisterValueChangedCallback(_ => ComparisonType = (Type)field.index);
            field.RegisterValueChangedCallback(_ => OnChangeValue());
            field.index = (int)ComparisonType;
        }

        void OnChangeValue()
        {
            switch (ComparisonType)
            {
                case Type.And:
                    _textMesh.text = "AND";
                    break;
                case Type.Or:
                    _textMesh.text = "OR";
                    break;
                case Type.Xor:
                    _textMesh.text = "XOR";
                    break;
            }
        }  
    }
}
