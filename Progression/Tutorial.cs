using System;
using ProgressionSystem.Helpers;
using UnityEngine;
using UnityEngine.Localization;
using Utility;

namespace ProgressionSystem
{
    [Serializable]
    public class Tutorial
    {
        public LocalizedString Name;
        public float HighlightDelay;
        public float ExplorationTime;
        public GameObject Animation;
        public Transform WorldspacePosition;

        [Tooltip("The requirements to start this tutorial.")]
        public Quest.CompletionRequirement[] StartRequirements;

        [Tooltip("The requirements to complete this tutorial.")]
        public Quest.CompletionRequirement[] CompletionRequirements;

        public Helper[] Helper;

        [DebugOnly] public bool Done;
    }

}
