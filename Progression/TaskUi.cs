using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace ProgressionSystem
{
    [RequireComponent(typeof(UIDocument))]
    public class TaskUi : MonoBehaviour
    {
        private UIDocument _ui;

        private readonly HashSet<Quest> _blinkingQuests = new();

        [SerializeField] private LocalizedString newQuest;
        [SerializeField] private LocalizedString yourTask;
        [SerializeField] private LocalizedString yourRewards;
        [SerializeField] private LocalizedString completed;
        [SerializeField] private LocalizedString failed;

        private void Awake()
        {
            _ui = GetComponent<UIDocument>();
            _ui.rootVisualElement.Q("QuestModal").visible = false;
        }

        public void PushTutorial(Tutorial tutorial, Action clickCallback)
        {
            AddTutorialButton(tutorial, clickCallback);
            UpdateList();
        }

        public void PushQuest(Quest quest)
        {
            StartCoroutine(HighlightQuest(quest));
            UpdateList();
        }

        public void PopTutorial(Tutorial tutorial)
        {
            tutorial.Done = true;
            UpdateList();
        }

        public void PopQuest(Quest quest)
        {
            StartCoroutine(ShowQuestModal(quest));
            UpdateList();
        }

        private void UpdateList()
        {
            var tasks = _ui.rootVisualElement.Q("Quests");
            we_love_collections:
            foreach (var element in tasks.Children())
            {
                switch (element.userData)
                {
                    case Quest q:
                        switch (q.CurrentState)
                        {
                            case Quest.State.Open:
                                tasks.Remove(element);
                                goto we_love_collections;
                            case Quest.State.Success:
                                tasks.Remove(element);
                                goto we_love_collections;
                            case Quest.State.Failure:
                                element.AddToClassList("questbutton--failure");
                                break;
                            case Quest.State.InProgress:
                            default:
                                break;
                        }
                        continue;
                    case Tutorial { Done: true }:
                        tasks.Remove(element);
                        goto we_love_collections;
                }
            }
        }

        private void AddTutorialButton(Tutorial tutorial, Action clickCallback)
        {
            var quests = _ui.rootVisualElement.Q("Quests");
            var button = new Button
            {
                userData = tutorial
            };
            button.AddToClassList("questbutton");
            var top = new Label();
            top.AddToClassList("questbutton--top");
            button.Add(top);
            tutorial.Name.StringChanged += newName => top.text = newName;
            button.clicked += clickCallback;
            quests.Add(button);
        }

        private IEnumerator HighlightQuest(Quest quest)
        {
            var quests = _ui.rootVisualElement.Q("Quests");
            var button = new Button
            {
                userData = quest
            };
            button.AddToClassList("questbutton");
            var top = new Label();
            top.AddToClassList("questbutton--top");
            button.Add(top);
            quest.Name.StringChanged += newName => top.text = newName;
            _blinkingQuests.Add(quest);
            button.clicked += () => StartCoroutine(ShowQuestModal(quest));
            quests.Add(button);
            yield return null;

            while (_blinkingQuests.Contains(quest))
            {
                button.AddToClassList("questbutton--highlight");
                yield return new WaitForSecondsRealtime(0.8f);
                button.RemoveFromClassList("questbutton--highlight");
                yield return new WaitForSecondsRealtime(0.8f);
            }
        }

        private IEnumerator ShowQuestModal(Quest quest)
        {
            yield return null;

            _blinkingQuests.Remove(quest);
            var questModal = _ui.rootVisualElement.Q("QuestModal");
            var okButton = questModal.Q<Button>("Ok");
            var header = questModal.Q<Label>("ModalHeader");
            var message = questModal.Q<Label>("Message");
            var rewards = questModal.Q<Label>("Rewards");
            Time.timeScale = 0;

            var modalClosed = false;

            void OnOkButtonClicked()
            {
                modalClosed = true;
            }

            if (quest.CurrentState == Quest.State.InProgress)
            {
                newQuest.StringChanged += value => header.text = value;
                quest.Name.StringChanged += value =>
                {
                    var messageBuilder = new StringBuilder(value).AppendLine();
                    messageBuilder.AppendLine("\n").AppendLine(yourTask.GetLocalizedString());
                    foreach (var requirement in quest.Wrapper.CompletionRequirements)
                    {
                        messageBuilder.AppendLine(requirement.Description.GetLocalizedString()).Append("\n");
                    }

                    message.text = messageBuilder.ToString();
                };
                rewards.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            else
            {
                completed.StringChanged += value =>
                {
                    header.text =
                        $"Quest {(quest.CurrentState == Quest.State.Success ? value : failed.GetLocalizedString())}";
                    message.text = quest.CurrentState == Quest.State.Success
                        ? quest.Success.GetLocalizedString()
                        : quest.Failure.GetLocalizedString();
                };

                var rewardsText = new StringBuilder();
                yourRewards.StringChanged += value =>
                {
                    rewardsText.AppendLine(value);
                    foreach (var entity in quest.Wrapper.Rewards.Entities)
                    {
                        rewardsText.Append(" - ").AppendLine(entity.name);
                    }

                    if (quest.Wrapper.Rewards.PreplacedTiles.Length > 0)
                    {
                        rewardsText.Append(" - ").AppendLine($"{quest.Wrapper.Rewards.PreplacedTiles.Length} Tiles");
                    }

                    foreach (var module in quest.Wrapper.Rewards.UnlockedModules)
                    {
                        rewardsText.Append(" - ").Append(module.GetType().Name).Append("\n");
                    }

                    rewards.text = rewardsText.ToString();
                };
                rewards.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }

            questModal.visible = true;

            okButton.clicked += OnOkButtonClicked;
            yield return new WaitUntil(() => modalClosed);

            okButton.clicked -= OnOkButtonClicked;
            questModal.visible = false;
            yield return null;

            Time.timeScale = 1;
        }
    }
}
