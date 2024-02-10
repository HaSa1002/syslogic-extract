using System;
using Cinemachine;
using Macrolayer;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace Microlayer
{
    public class Brain : MonoBehaviour
    {
        public Transform Target;
        [DebugOnly] public MacroEntity TargetEntity;
        [SerializeField] private bool _active;

        /// <summary>
        /// Callback when the brain gets visible (true) or hidden (false)
        /// </summary>
        public Action<bool> OnBrainVisibilityChanged;

        public void SetActive(bool active)
        {
            _active = active;
            TargetEntity.enabled = active;
            TargetEntity.SetActive(active);
        }

        public bool GetActive()
        {
            return _active;
        }

        [SerializeField] private ColourPalette ColourPalette;

        [FormerlySerializedAs("EntityCamera")] [SerializeField] private CinemachineVirtualCamera PreviewCamera;

        private bool _wasHidden;
        private void Start()
        {
            Debug.Assert(Target, "Please assign a target transform", this);
            Debug.Assert(PreviewCamera, "Please assign a preview camera", this);

            SetActive(_active);
        }

        public void Show()
        {
            var control = GetComponentInParent<BrainControl>();
            _wasHidden = false;
            control.RequestFocus(this);
            foreach (var aRenderer in GetComponentsInChildren<Renderer>(true))
            {
                aRenderer.gameObject.layer = LayerMask.NameToLayer("Microlayer");
            }
            PreviewCamera.Priority = 11;
            transform.parent.GetComponentInChildren<Skybox>().material = ColourPalette.Background;
            OnBrainVisibilityChanged?.Invoke(true);
        }

        public void Hide()
        {
            if (_wasHidden) return;

            _wasHidden = true;
            ReHide();
            OnBrainVisibilityChanged?.Invoke(false);
        }

        public void ReHide()
        {
            foreach (var aRenderer in GetComponentsInChildren<Renderer>(true))
            {
                aRenderer.gameObject.layer = LayerMask.NameToLayer("Hidden");
            }
        }
    }
}