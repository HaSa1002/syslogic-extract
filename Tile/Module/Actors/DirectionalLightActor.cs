using UnityEngine;

namespace Microlayer
{
    public class DirectionalLightActor : LightActor
    {
        [SerializeField] private Light DirLight;

        private Camera _cam;
        private int _originalCullingMask;


        protected override void Start()
        {
            base.Start();
            _cam = Camera.main;
            Debug.Assert(_cam);
            _originalCullingMask = _cam!.cullingMask;
        }


        public override void Execute()
        {
            base.Execute();
            UpdateEvidenceVisibility(Intensity);
            DirLight.intensity = Intensity;
        }


        public override void OnGraphRebuildRequested()
        {
            base.OnGraphRebuildRequested();
            UpdateEvidenceVisibility(Intensity);
            DirLight.intensity = Intensity;
        }


        private void UpdateEvidenceVisibility(float input)
        {
            if (!DirLight) return;
            if (input > 0f)
            {
                _cam.cullingMask |= 1 << 13;
            }
            else
            {
                _cam.cullingMask = _originalCullingMask;
            }

        }
    }
}
