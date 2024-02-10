using System;
using Microlayer;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using Utility;

namespace ProgressionSystem
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Data/Quest")]
    public class Quest : ScriptableObject
    {
        [Serializable]
        public class CompletionRequirement
        {
            public enum Operator
            {
                GreaterEqual,
                Greater,
                Equal,
                Lesser,
                LesserEqual
            }

            [Tooltip("A GameObject that implements IProgressionEmitter like Actors. Can also be used for custom progression emitters.")]
            public GameObject Emitter;

            [Tooltip("The value that should be reached. The actual value is compared using the comparison method below.")]
            public float TargetValue = float.NaN;

            [Tooltip("The operator used to compare the current value with the target.")]
            public Operator Comparison;

            [Tooltip("The description that should be shown in the UI.")]
            public LocalizedString Description;

            [DebugOnly] public float CurrentValue = float.NaN;
        }

        [Serializable]
        public struct Reward
        {
            [Tooltip("Set an entity that is also set in GridMap.entities that should get activated. " +
                     "Activating an entity also activates all associated entities except for those that are set to be IndependentStateActive. " +
                     "Those can be activated with PreplacedTiles below.")]
            public Transform[] Entities;
            [Tooltip("Assign tile derived objects (eg modules) that should get activated. " +
                     "Note this is only relevant for tiles that are IndependentStateActive. " +
                     "All other tiles will be activated by activating the entity.")]
            public Tile[] PreplacedTiles;
            [Tooltip("Assign prefabs whose root has the unlockable module assigned. Only unlocked modules are accessible in the UI.")]
            public Module[] UnlockedModules;
            [Tooltip("Derive a script from RewardReceiver and assign the game object here if you need custom logic to drive a reward.")]
            public RewardReceiver[] RewardReceivers;
        }

        [Serializable]
        public struct Prerequisite
        {
            [Serializable]
            public struct PreviousQuest
            {
                [Tooltip("Set the quest that should be checked against.")]
                public Quest Quest;
                [Tooltip("Set the state the quest needs to have.")]
                public State State;
            }
            [Tooltip("Set entities that are also in GridMap.entities that need to be active in order to unlock this quest.")]
            public Transform[] UnlockedEntities;
            [Tooltip("Select quests that need to have a specific state to unlock this quest.")]
            public PreviousQuest[] PreviousQuests;
        }

        public enum State
        {
            Open,
            InProgress,
            Success,
            Failure
        }

        [Tooltip("The name of the quest.")]
        public LocalizedString Name;

        [Tooltip("The message that should be shown if the quest was successfully completed.")]
        public LocalizedString Success;

        [Tooltip("The message that should be shown on failure.")]
        public LocalizedString Failure;

        [Tooltip("Doesn't show the start quest window.")]
        public bool SkipStartExplanation;

        [DebugOnly]
        public State CurrentState;

        [DebugOnly] public QuestWrapper Wrapper;
    }

    [Serializable]
    public class QuestWrapper
    {
        [Tooltip("The quest that we extend to provide progression features.")]
        public Quest Quest;

        [Tooltip("The prerequisites that need to be fulfilled to enable this quest.")]
        public Quest.Prerequisite Prerequisites;

        [Tooltip("The requirements to complete this quest.")]
        public Quest.CompletionRequirement[] CompletionRequirements;

        [Tooltip("The rewards obtained by this quest.")]
        public Quest.Reward Rewards;
    }
}
