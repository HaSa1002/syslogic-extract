using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Microlayer
{
    public class CompareModule : Logic
    {
        public enum Type
        {
            Less,
            Equal,
            Greater,
        }

        public Type ComparisonType = Type.Less;
        private bool _firstUiRun = true;
        private TextMeshPro _textMesh;
        [FormerlySerializedAs("_rotParent")] [SerializeField] private Transform RotParent;
        [FormerlySerializedAs("_compareParent")] [SerializeField] private Transform CompareParent;

        protected override void Start()
        {
            base.Start();

            _textMesh = GetComponentInChildren<TextMeshPro>();
        }

        private void Update()
        {
            var dot = Vector3.Dot(ConnectionPorts[0].transform.forward, RotParent.forward);
            if (dot < 0)
            {
                CompareParent.localRotation = Quaternion.Euler(-58.5f, 0f, 180f);
            }
            else
            {
                CompareParent.localRotation = Quaternion.Euler(-58.5f, 0f, 0f);
            }
        }

        public override void GarbageExecute()
        {
            for (int i = 0; i < 2 - Input.Count; i++)
            {
                Input.Add(Random.value);
            }
            Execute();
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            switch (ComparisonType)
            {
                case Type.Less:
                    var less = new[] { (MicroData)(Input[0] < Input[1]) };
                    PushData(less);
                    UpdateEmissionValue(less);
                    break;
                case Type.Equal:
                    var equal = new[] { (MicroData)Approximately((float)Input[0],(float)Input[1], 0.05f) }; //Approximately
                    PushData(equal);
                    UpdateEmissionValue(equal);
                    break;
                case Type.Greater:
                    var greater = new[] { (MicroData)(Input[0] > Input[1]) };
                    PushData(greater);
                    UpdateEmissionValue(greater);
                    break;
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
                case Type.Less:
                    _textMesh.text = "<";
                    break;
                case Type.Equal:
                    _textMesh.text = "=";
                    break;
                case Type.Greater:
                    _textMesh.text = ">";
                    break;
            }
        }

        public static bool Approximately(float a, float b, float threshold)
        {
            if (threshold > 0f)
            {
                return Mathf.Abs(a- b) <= threshold;
            }
            return Mathf.Approximately(a, b);
        }
    }
}
