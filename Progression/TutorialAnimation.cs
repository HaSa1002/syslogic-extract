using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;

namespace ProgressionSystem
{
    public class TutorialAnimation : MonoBehaviour
    {
        public LocalizedString TutorialText;
        public Texture2D[] Textures;
        [SerializeField] private float[] FrameTimes;
        private Button _button;

        private void OnEnable()
        {
            var uiDoc = GetComponent<UIDocument>().rootVisualElement;
            _button = uiDoc.Q<Button>();
            var label = uiDoc.Q<Label>();
            _button.clicked += () => gameObject.SetActive(false);
            StartCoroutine(PlayAnimation());
            
            label.style.display = TutorialText.IsEmpty ? DisplayStyle.None : DisplayStyle.Flex;
            TutorialText.StringChanged += text => label.text = text;
            }

        private IEnumerator PlayAnimation()
        {
            while ((bool)this)
            {
                for (int i = 0; i < FrameTimes.Length; i++)
                {
                    _button.style.backgroundImage = new StyleBackground(Textures[i]);
                    yield return new WaitForSeconds(FrameTimes[i] / 1000f);
                }
            }
        }
    }
}
