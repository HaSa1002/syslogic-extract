using UnityEngine;
using System.Collections.Generic;
using Macrolayer;

namespace Microlayer
{
    public class CameraActor : Actor
    {
        private const float MaxPhotoDistance = 1.4f;
        private bool _activate;
        private bool _shutter;
        private RobiCameraControl _control;
        private bool _clearShutter;

        public Camera MacroCamera;
        public List<Evidence> EvidenceList = new();

        protected override void Start()
        {
            _control = GetComponent<RobiCameraControl>();
            base.Start();
        }
        public override void GarbageExecute()
        {
            for (int i = 0; i <= 2 - Input.Count; i++)
            {
                Input.Add(false);
            }
            Execute();
        }

        public override void OnGraphRebuildRequested()
        {
            base.OnGraphRebuildRequested();
            _activate = false;
            _shutter = false;
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            _activate = (bool)Input[0];
            _shutter = (bool)Input[1];
        }

        private void Update()
        {
            if (!_shutter)
            {
                _clearShutter = true;
            }

            _control.ControlRobiCamera(_activate);
            if (!_activate) return;
            if (!_shutter || !_clearShutter) return;

            _clearShutter = false;
            CapturePicture();
        }

        private void CapturePicture()
        {
            foreach (var evidence in EvidenceList)
            {
                if (!IsTargetVisible(evidence)) continue;

                var visible = evidence.Captured = IsTargetInFrustum(evidence);
                if (visible)
                {
                    switch (evidence.TypeOfEvidence)
                    {
                        case Evidence.Type.Footsteps:
                            EmitProgressionValue(0f);
                            break;
                        case Evidence.Type.Blood:
                            EmitProgressionValue(1f);
                            break;
                        case Evidence.Type.Weapon:
                            Debug.Log("no weapon evidence implemented yet");
                            break;
                    }
                }
                Debug.Log("Motive " + evidence.TypeOfEvidence + " is visible " + visible);
            }
            _control.StartCoroutine(_control.CameraShutter());
        }

        private bool IsTargetInFrustum(Evidence evidence)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(MacroCamera);
            var point = evidence.transform.position;
            foreach (var plane in planes)
            {
                if (!(plane.GetDistanceToPoint(point) > 0f) ||
                    !(plane.GetDistanceToPoint(point) < MaxPhotoDistance)) continue;

                if (GeometryUtility.TestPlanesAABB(planes, evidence.Collider.bounds))
                    return IsTargetInViewport(evidence, MacroCamera);
            }
            return false;
        }

        private static bool IsTargetInViewport(Evidence evidence, Camera camera)
        {
            var viewPos = camera.WorldToViewportPoint(evidence.transform.position);
            return viewPos.x is >= 0 and <= 1 && viewPos.y is >= 0 and <= 1 && viewPos.z > 0;
        }

        private static bool IsTargetVisible(Evidence evidence)
        {
            return evidence.TypeOfEvidence switch
            {
                Evidence.Type.Footsteps => true,
                Evidence.Type.Blood => evidence.UVLight && evidence.UVLight.Intensity > 0f,
                Evidence.Type.Weapon => evidence.WeaponMesh.enabled,
                _ => false
            };
        }
    }
}