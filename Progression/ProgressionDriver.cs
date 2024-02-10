using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microlayer;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProgressionSystem
{
    public class ProgressionDriver : MonoBehaviour
    {
        public bool SkipQuest;
        public QuestWrapper[] Quests;
        public TaskUi TaskUi;

        private readonly HashSet<Quest> _activeQuests = new();
        private BrainControl _brainControl;
        private bool _questUpdated;
        private readonly Queue<Quest> _questUiOrder = new();
        private Button _activeQuestButton;

        private bool CheckPrerequisites(Quest quest)
        {
            foreach (var entity in quest.Wrapper.Prerequisites.UnlockedEntities)
            {
                foreach (var brain in _brainControl.Brains)
                {
                    if (brain.Target != entity) continue;

                    if (!brain.GetActive()) return false;
                    break;
                }
            }

            return quest.Wrapper.Prerequisites.PreviousQuests.All(previousQuest => previousQuest.Quest.CurrentState == previousQuest.State);
        }

        private void CompleteQuest(Quest quest)
        {
            _activeQuests.Remove(quest);

            quest.CurrentState = Quest.State.Success;

            // Yeah no disconnect here for now.

            foreach (var entity in quest.Wrapper.Rewards.Entities)
            {
                _brainControl.ActivateEntity(_brainControl.Brains.First(brain => brain.Target == entity));
            }

            foreach (var tile in quest.Wrapper.Rewards.PreplacedTiles)
            {
                tile.Grid.EnableTile(tile);
            }

            foreach (var module in quest.Wrapper.Rewards.UnlockedModules)
            {
                _brainControl.Unlock(module.GetType());
            }

            foreach (var receiver in quest.Wrapper.Rewards.RewardReceivers)
            {
                receiver.ReceiveReward(quest);
            }

            TaskUi.PopQuest(quest);
        }

        private void StartQuest(Quest quest)
        {
            _activeQuests.Add(quest);
            quest.CurrentState = Quest.State.InProgress;
            if (quest.SkipStartExplanation) return;

            TaskUi.PushQuest(quest);
        }

        private void Start()
        {
            _brainControl = GetComponent<BrainControl>();
            var controls = ControlsWrapper.Controls;
            controls.General.SkipQuest.performed += _ => SkipQuest = true;
            foreach (var quest in Quests)
            {
                quest.Quest.CurrentState = Quest.State.Open;
                quest.Quest.Wrapper = quest;
                foreach (var requirement in quest.CompletionRequirements)
                {
                    Debug.Assert(requirement.Emitter, $"Quest '{quest.Quest.Name}' completion requirements require emitters that implement IProgressionEmitter like Actors.", this);
                    var emitter = requirement.Emitter.GetComponent<IProgressionEmitter>();
                    Debug.Assert(emitter != null, $"Quest '{quest.Quest.Name}' completion requirements require emitters that implement IProgressionEmitter like Actors.", this);
                    // we can't unbind them. That's meh.
                    // Alternately, we could bind to a member function and iterate over all quests on callback...
                    // yeah.
                    // If this proves to be to perf heavy, we should consider changing it. I don't think it's gonna be that bad
                    emitter.ProgressionValueChanged += value =>
                    {
                        _questUpdated = true;
                        requirement.CurrentValue = value;
                    };
                    emitter.ProgressionStateChanged += success =>
                    {
                        _questUpdated = true;
                        if (success)
                        {
                            requirement.CurrentValue = requirement.TargetValue;
                        }
                        else
                        {
                            quest.Quest.CurrentState = Quest.State.Failure;
                        }
                    };
                    requirement.CurrentValue = float.NaN;
                }

                if (CheckPrerequisites(quest.Quest))
                {
                    StartQuest(quest.Quest);
                }
            }
            //RebuildActiveQuests();
        }

        private void Update()
        {
            var questCompleted = false;
            if (SkipQuest)
            {
                SkipFirstQuest();
                questCompleted = true;
            }
            if (!_questUpdated) return;

            _questUpdated = false;
            search_again:
            foreach (var activeQuest in _activeQuests)
            {
                if (activeQuest.CurrentState == Quest.State.Failure)
                {
                    questCompleted = true;
                    CompleteQuest(activeQuest);
                    goto search_again;
                }

                var completed = true;
                foreach (var requirement in activeQuest.Wrapper.CompletionRequirements)
                {
                    completed = requirement.Comparison switch
                    {
                        Quest.CompletionRequirement.Operator.GreaterEqual => requirement.CurrentValue >= requirement.TargetValue,
                        Quest.CompletionRequirement.Operator.Greater => requirement.CurrentValue > requirement.TargetValue,
                        Quest.CompletionRequirement.Operator.Equal => Mathf.Approximately(requirement.CurrentValue, requirement.TargetValue),
                        Quest.CompletionRequirement.Operator.Lesser => requirement.CurrentValue < requirement.TargetValue,
                        Quest.CompletionRequirement.Operator.LesserEqual => requirement.CurrentValue <= requirement.TargetValue,
                        _ => true // Is actually a bug but let's not care about it here. (Shouldn't happen anyway)
                    };

                    if (!completed) break;
                }

                if (!completed) continue;

                questCompleted = true;
                CompleteQuest(activeQuest);
                goto search_again;
            }

            if (!questCompleted) return;

            // We may have quests that start based on a state of another starting quest. By doing another pass, we catch'em all.
            for (var checkAgain = true; checkAgain;)
            {
                checkAgain = false;
                foreach (var quest in Quests)
                {
                    if (quest.Quest.CurrentState != Quest.State.Open) continue;
                    if (!CheckPrerequisites(quest.Quest)) continue;

                    StartQuest(quest.Quest);
                    checkAgain = true;
                }
            }

            //RebuildActiveQuests();

            if (_activeQuests.Count == 0)
            {
                TaskUi.GetComponent<UIDocument>().rootVisualElement.Q("ActiveQuests").visible = false;
                print("Sandbox mode activated.");
                // whatever we do here.
            }
        }

        private void SkipFirstQuest()
        {
            SkipQuest = false;
            foreach (var activeQuest in _activeQuests)
            {
                CompleteQuest(activeQuest);
                _questUpdated = true;
                break;
            }
        }
    }
}
