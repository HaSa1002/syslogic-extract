using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Microlayer
{
    public class ColourSensor : Sensor
    {
        private Texture2D _sensorTexture;
        private readonly MicroData[] _colours = new MicroData[4]; //CMYK
        private Camera _sensorCamera;
        private readonly VisualElement[] _colourPreviews = new VisualElement[5];

        private static readonly Vector2[] SamplePoints = new[]
        {
            new Vector2(0.5f, 0.5f),
            // new Vector2(0.1f, 0.1f),
            // new Vector2(0.1f, 0.9f),
            // new Vector2(0.9f, 0.1f),
            // new Vector2(0.9f, 0.9f)
        };

        protected override void Start()
        {
            base.Start();
            _sensorCamera = MacroSensorPosition.GetComponentInChildren<Camera>();
            if (!_sensorCamera || !_sensorCamera.targetTexture)
            {
                Debug.LogAssertion("Failed to obtain sensor camera and its render texture!", this);
                enabled = false;
            }

            var targetTexture = _sensorCamera.targetTexture;
            _sensorTexture = new Texture2D(targetTexture.width, targetTexture.height);

            RenderPipelineManager.endCameraRendering += OnRenderPipelineManagerCameraRenderingEnded;

            var ui = ModuleInfo.rootVisualElement;
            _colourPreviews[0] = ui.Q<VisualElement>("PreviewColour");
            _colourPreviews[1] = ui.Q<VisualElement>("PreviewCyan");
            _colourPreviews[2] = ui.Q<VisualElement>("PreviewMagenta");
            _colourPreviews[3] = ui.Q<VisualElement>("PreviewYellow");
            _colourPreviews[4] = ui.Q<VisualElement>("PreviewKey");
        }

        private void OnRenderPipelineManagerCameraRenderingEnded(ScriptableRenderContext context, Camera cam)
        {
            if (cam != _sensorCamera) return;

            _sensorTexture.ReadPixels(cam.pixelRect, 0, 0);

            for (int i = 0; i < 4; i++)
            {
                _colours[i] = 0;
            }

            var value = _sensorTexture.GetPixelBilinear(SamplePoints[0].x, SamplePoints[0].y);

            for (int i = 0; i < SamplePoints.Length; i++)
            {
                value = _sensorTexture.GetPixelBilinear(SamplePoints[i].x, SamplePoints[i].y);
                _colours[3] += value.maxColorComponent;
                _colours[0] += value.r;
                _colours[1] += value.g;
                _colours[2] += value.b;
            }

            for (int i = 0; i < 4; i++)
            {
                _colours[i] /= SamplePoints.Length;
            }

            _colourPreviews[0].style.backgroundColor = value;

            var k = 1 - _colours[3];
            _colours[3] = k; // maybe value.grayscale (see above. K would be saved separately)
            _colours[0] = (1 - _colours[0] - k) / (1 - k);
            _colours[1] = (1 - _colours[1] - k) / (1 - k);
            _colours[2] = (1 - _colours[2] - k) / (1 - k);

            _colourPreviews[1].style.backgroundColor = Color.Lerp(Color.white, Color.cyan, (float)_colours[0]);
            _colourPreviews[2].style.backgroundColor = Color.Lerp(Color.white, Color.magenta, (float)_colours[1]);
            _colourPreviews[3].style.backgroundColor = Color.Lerp(Color.white, Color.yellow, (float)_colours[2]);
            _colourPreviews[4].style.backgroundColor = new Color(1 - (float)k, 1 - (float)k, 1 - (float)k);
            //print($"C: {_colours[0]}, M: {_colours[1]}, Y: {_colours[2]}, K: {_colours[3]} ## {value}");

        }

        private void OnDestroy()
        {
            RenderPipelineManager.endCameraRendering -= OnRenderPipelineManagerCameraRenderingEnded;
        }

        public override void Execute()
        {
            if (!isActiveAndEnabled)
            {
                GarbageExecute();
                return;
            }

            PushData(_colours);
            UpdateEmissionValue(_colours);
        }
    }
}