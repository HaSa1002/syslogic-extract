using UnityEngine;
using Utility;

namespace Microlayer
{
    public class LightActor : Actor
    {
        [Tooltip("Used to indicate which light component should be selected in the entity."), SerializeField]
        private int ChildIndex;

        private Light _light;
        private float _maxIntensity;
        [DebugOnly] public float Intensity; // FIXME: The camera actor shouldn't access this property


        protected override void Start()
        {
            base.Start();
            _light = Target.GetComponentsInChildren<Light>(true)[ChildIndex];
            _maxIntensity = _light.intensity;
            _light.enabled = true;
            _light.intensity = 0;
        }


        public override void Execute()
        {
            Intensity = (float)Input[0];
            _light.intensity = _maxIntensity * Intensity;
            EmitProgressionValue();
            UpdateEmissionValue(Input);
        }


        public override void OnGraphRebuildRequested()
        {
            Intensity = 0f;
            _light.intensity = _maxIntensity * Intensity;
            UpdateEmissionValue(Intensity);
            base.OnGraphRebuildRequested();
        }
    }
}
