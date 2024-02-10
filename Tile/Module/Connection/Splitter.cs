using UnityEngine;
using UnityEngine.UIElements;

namespace Microlayer
{
    public class Splitter : ExecutableModule
    {
        private float _limit = 1;
        private bool _firstUiRun = true;
        public MeshRenderer[] Faces = {null, null, null, null};
        [SerializeField]
        private MeshRenderer _antenna;
        private Material[] _glowMaterials = new Material[5];
        private readonly Material[] _eyeMaterials = new Material[4];
        private static readonly int Offset = Shader.PropertyToID("_Offset");

        private bool _addTileCalled = false;

        public override void OnAddTile()
        {
            base.OnAddTile();

            if (_addTileCalled) return;

            GlowMaterial = Grid.ColourPalette.LogicGlow;

            for (var i = 0; i < Faces.Length; i++)
            {
                var mats = Faces[i].sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j] != GlowMaterial)
                    {
                        mats[j] = Instantiate(mats[j]);
                        _eyeMaterials[i] = mats[j];
                        continue;
                    }
                    mats[j] = Instantiate(GlowMaterial);
                    _glowMaterials[i] = mats[j];
                }

                Faces[i].sharedMaterials = mats;
            }

            var materials = _antenna.sharedMaterials;
            for (int k = 0; k < _antenna.sharedMaterials.Length; k++)
            {
                if (_antenna.sharedMaterials[k] == GlowMaterial)
                {
                    materials[k] = Instantiate(GlowMaterial);
                    _glowMaterials[4] = materials[k];
                }
            }
            _antenna.sharedMaterials = materials;

            _addTileCalled = true;
        }

        public override void Execute()
        {
            var output = Input[0] < _limit ? Input[0] : _limit;
            UpdateFaces((float)output);
            PushData(new[] { output, output, output });
        }

        public override void ShowUi()
        {
            base.ShowUi();

            if (!_firstUiRun) return;

            var slider = ModuleInfo.rootVisualElement.Q<Slider>();
            slider.value = _limit;
            slider.RegisterValueChangedCallback(evt => _limit = evt.newValue);
            _firstUiRun = false;
        }

        protected override void UpdateVisualState(bool running)
        {
            base.UpdateVisualState(running);
            if (running) return;
            UpdateFaces(0);
        }

        private void UpdateFaces(float value)
        {
            for (int i = 0; i < Faces.Length; i++)
            {
                if (!ConnectedModules[i])
                {
                    UpdateEmissionValue(0f, _glowMaterials[i]);
                    _eyeMaterials[i].SetVector(Offset, new Vector4(0, 0.5f));
                    continue;
                }
                UpdateEmissionValue(value, _glowMaterials[i]);
                _eyeMaterials[i].SetVector(Offset, new Vector4(0, 0f));
            }

            UpdateEmissionValue(value, _glowMaterials[4]);
        }
    }
}