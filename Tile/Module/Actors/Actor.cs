using ProgressionSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Microlayer
{
    public abstract class Actor : ExecutableModule, IProgressionEmitter
    {
        [FormerlySerializedAs("target")] [HideInInspector]
        public Transform Target;

        public event IProgressionEmitter.ProgressionValueChangedHandler ProgressionValueChanged;
#pragma warning disable CS0067

        // we never gonna use custom states in actors
        public event IProgressionEmitter.ProgressionStateChangedHandler ProgressionStateChanged;
#pragma warning restore CS0067

        public override void OnAddTile()
        {
            base.OnAddTile();

            _glowMat = Instantiate(GlowMaterial);

            SetGlowMaterial();
        }

        protected void EmitProgressionValue(MicroData value)
        {
            ProgressionValueChanged?.Invoke((float)value);
        }

        protected void EmitProgressionValue()
        {
            ProgressionValueChanged?.Invoke((float)Input[0]);
        }
    }
}
