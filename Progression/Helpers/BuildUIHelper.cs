using System;
using System.Collections;
using Macrolayer;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProgressionSystem.Helpers
{
    public class BuildUIHelper : Helper
    {
        private BuildUIEventHandler _buildUI;
        private bool _playAnimation;

        private void Start()
        {
            _buildUI = GetComponent<BuildUIEventHandler>();
        }

        public override void Highlight()
        {
            StartCoroutine(HighlightAnimation());
        }

        public override void Hide()
        {
            _playAnimation = false;
        }

        private IEnumerator HighlightAnimation()
        {
            _playAnimation = true;

            while (_playAnimation)
            {
                _buildUI.DismantleButton.AddToClassList("buttonhelper--yellow");
                yield return new WaitForSecondsRealtime(0.8f);
                _buildUI.DismantleButton.RemoveFromClassList("buttonhelper--yellow");
                yield return new WaitForSecondsRealtime(0.8f);
            }
        }
    }
}
