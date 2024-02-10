using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProgressionSystem
{
    public class TutorialDriver : MonoBehaviour
    {
        [SerializeField] private TaskUi TaskUi;
        public List<Tutorial> Tutorials;
        public List<Tutorial> ContextHelpers;

        private Tutorial _currentTutorial;
        private float _exploredTime;

        private bool _helpersShown;
        private bool _animationShown;

        private Button _activeQuestButton;
        private GameObject _currentAnimation;

        private Tutorial _lastTutorial;

        private void Start()
        {
            foreach (var tutorial in Tutorials)
            {
                ConnectRequirements(tutorial.CompletionRequirements, tutorial.Name.GetLocalizedString());
                ConnectRequirements(tutorial.StartRequirements, tutorial.Name.GetLocalizedString());
            }
            foreach (var tutorial in ContextHelpers)
            {
                ConnectRequirements(tutorial.CompletionRequirements, tutorial.Name.GetLocalizedString());
                ConnectRequirements(tutorial.StartRequirements, tutorial.Name.GetLocalizedString());
            }
        }

        private void Update()
        {
            foreach (var helper in ContextHelpers.Where(helper => AllRequirementsMet(helper.StartRequirements)))
            {
                if (_lastTutorial != null)
                {
                    TaskUi.PopTutorial(_lastTutorial);
                    _lastTutorial.Done = false;
                }
                Tutorials.Insert(0, helper);
                ContextHelpers.Remove(helper);
                break;
            }

            if (Tutorials.Count == 0)
            {
                enabled = ContextHelpers.Count > 0;
                return;
            }

            var currentTutorial = Tutorials[0];
            if (currentTutorial != _lastTutorial && AllRequirementsMet(Tutorials[0].StartRequirements))
            {
                TaskUi.PushTutorial(currentTutorial,
                    () =>
                    {
                        _exploredTime = currentTutorial.ExplorationTime + 1;
                        if (!_currentAnimation) return;
                        _currentAnimation.SetActive(true);
                    });
                _lastTutorial = currentTutorial;
            }

            if (currentTutorial == _lastTutorial)
            {
                UpdateCurrentQuest();
            }

            search_again:
            foreach (var tutorial in Tutorials.Where(tutorial => OneRequirementMet(tutorial.CompletionRequirements)))
            {
                CompleteTutorial(tutorial);
                goto search_again;
            }
        }

        private void UpdateCurrentQuest()
        {
            _exploredTime += Time.deltaTime;
            if (!_helpersShown && _exploredTime > Tutorials[0].HighlightDelay)
            {
                _helpersShown = true;
                foreach (var helper in Tutorials[0].Helper)
                {
                    helper.Highlight();
                }
            }

            if (!_animationShown && _exploredTime > Tutorials[0].ExplorationTime)
            {
                _animationShown = true;
                _currentAnimation = Instantiate(Tutorials[0].Animation, transform);
                _currentAnimation.layer = gameObject.layer;
                if (Tutorials[0].WorldspacePosition)
                {
                    _currentAnimation.transform.position = Tutorials[0].WorldspacePosition.position;
                }
            }
        }

        private void CompleteTutorial(Tutorial tutorial)
        {
            if (tutorial != Tutorials[0])
            {
                Tutorials.Remove(tutorial);
                return;
            }

            Tutorials.RemoveAt(0);
            foreach (var helper in tutorial.Helper)
            {
                helper.Hide();
            }

            Destroy(_currentAnimation);

            _helpersShown = false;
            _animationShown = false;
            _exploredTime = 0;
            _lastTutorial = null;
            TaskUi.PopTutorial(tutorial);
        }

        private void ConnectRequirements(IEnumerable<Quest.CompletionRequirement> completionRequirements, string questName)
        {
            foreach (var requirement in completionRequirements)
            {
                Debug.Assert(requirement.Emitter, $"Quest '{questName}' completion requirements require emitters that implement IProgressionEmitter like Actors.", this);
                var emitter = requirement.Emitter.GetComponent<IProgressionEmitter>();
                Debug.Assert(emitter != null, $"Quest '{questName}' completion requirements require emitters that implement IProgressionEmitter like Actors.", this);
                // we can't unbind them. That's meh.
                // Alternately, we could bind to a member function and iterate over all quests on callback...
                // yeah.
                // If this proves to be to perf heavy, we should consider changing it. I don't think it's gonna be that bad
                emitter.ProgressionValueChanged += value =>
                {
                    requirement.CurrentValue = value;
                };
                emitter.ProgressionStateChanged += success =>
                {
                    if (success)
                    {
                        requirement.CurrentValue = requirement.TargetValue;
                    }
                };
                requirement.CurrentValue = float.NaN;
            }
        }

        private static bool AllRequirementsMet(IEnumerable<Quest.CompletionRequirement> requirements)
        {
            return requirements.Select(requirement => requirement.Comparison switch
                {
                    Quest.CompletionRequirement.Operator.GreaterEqual => requirement.CurrentValue >= requirement.TargetValue,
                    Quest.CompletionRequirement.Operator.Greater => requirement.CurrentValue > requirement.TargetValue,
                    Quest.CompletionRequirement.Operator.Equal => Mathf.Approximately(requirement.CurrentValue, requirement.TargetValue),
                    Quest.CompletionRequirement.Operator.Lesser => requirement.CurrentValue < requirement.TargetValue,
                    Quest.CompletionRequirement.Operator.LesserEqual => requirement.CurrentValue <= requirement.TargetValue,
                    _ => true // Is actually a bug but let's not care about it here. (Shouldn't happen anyway)
                })
                .All(completed => completed);
        }

        private static bool OneRequirementMet(IEnumerable<Quest.CompletionRequirement> requirements)
        {
            return requirements.Select(requirement => requirement.Comparison switch
                {
                    Quest.CompletionRequirement.Operator.GreaterEqual => requirement.CurrentValue >= requirement.TargetValue,
                    Quest.CompletionRequirement.Operator.Greater => requirement.CurrentValue > requirement.TargetValue,
                    Quest.CompletionRequirement.Operator.Equal => Mathf.Approximately(requirement.CurrentValue, requirement.TargetValue),
                    Quest.CompletionRequirement.Operator.Lesser => requirement.CurrentValue < requirement.TargetValue,
                    Quest.CompletionRequirement.Operator.LesserEqual => requirement.CurrentValue <= requirement.TargetValue,
                    _ => true // Is actually a bug but let's not care about it here. (Shouldn't happen anyway)
                })
                .Any(completed => completed);
        }
    }
}
