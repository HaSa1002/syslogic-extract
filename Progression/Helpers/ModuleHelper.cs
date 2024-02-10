using Microlayer;
using System.Collections;
using EPOOutline;
using UnityEngine;

namespace ProgressionSystem.Helpers
{
    public class ModuleHelper : Helper
    {
        public Module Module;
        private Outlinable _helperFill;

        [ColorUsage(true, true)]
        private static readonly Color HDRColor = new Vector4(1f, 235f/256f, 4f/256f, 0.3f);

        public override void Highlight()
        {
            if (!Module) return;

            SetupHelper();
            StartCoroutine(ModuleHelperAnimation());
        }

        public override void Hide()
        {
            if (!Module || !_helperFill) return;
            
            Destroy(_helperFill);
            _helperFill = null;
        }

        private IEnumerator ModuleHelperAnimation()
        {
            while (_helperFill)
            {
                _helperFill.OutlineParameters.FillPass.SetColor("_PublicColor", SineColor());
                yield return new WaitForEndOfFrame();
            }
        }

        private static Vector4 SineColor()
        {
            var alpha = 0.3f + Mathf.Sin(Time.time * 4f) * 0.3f;
            var color = new Color(HDRColor.r, HDRColor.g, HDRColor.b, alpha);
            return color;
        }

        private void SetupHelper()
        {
            _helperFill = Module.gameObject.AddComponent<Outlinable>();
            _helperFill.AddAllChildRenderersToRenderingList(RenderersAddingMode.MeshRenderer | RenderersAddingMode.SkinnedMeshRenderer);
            _helperFill.OutlineParameters.DilateShift = 0;
            _helperFill.OutlineParameters.BlurShift = 0;
            _helperFill.OutlineParameters.FillPass.Shader = Resources.Load<Shader>("Easy performant outline/Shaders/Fills/ColorFill");
        }
    }
}
