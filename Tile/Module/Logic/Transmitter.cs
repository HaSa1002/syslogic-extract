using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Microlayer
{
    public class Transmitter : Logic
    {
        public Receiver ReceiverModule;
        private int _outputPort = -1;
        private bool _unpaired = true;
        private bool _firstRun = true;

        public BrainControl BrainControl;

        public override void OnAddTile()
        {
            base.OnAddTile();
            //assign brain control
            BrainControl = GetComponentInParent<BrainControl>();

            for (var i = 0; i < 4; i++)
            {
                if (PortDefinitions[i] == PortType.Output)
                {
                    _outputPort = i;
                }
            }
        }

        public override void OnRemoveTile()
        {
            base.OnRemoveTile();
            if (_unpaired) return;

            ReceiverModule.Unpair();
        }


        public override void Execute()
        {
            UpdateEmissionValue(Input);
            PushData(Input);
        }

        public override void ShowUi()
        {
            base.ShowUi();

            if (!_firstRun) return;

            _firstRun = false;
            var button = ModuleInfo.rootVisualElement.Q<Button>();
            button.clicked += OnChangeValue;
            new LocalizedString("Misc", "pair").StringChanged += text => button.text = text;
        }

        private void OnChangeValue()
        {
            if(_unpaired)
            {
                BrainControl.EnterPairMode(this);
            }
            else
            {
                ReceiverModule.GetComponentInParent<Brain>().Show();
            }
            HideUi();
        }

        public void AssignReceiver(Receiver assignedReceiver)
        {
            ReceiverModule = assignedReceiver;
            ConnectedModules[_outputPort] = ReceiverModule;
            _unpaired = false;

            var button = ModuleInfo.rootVisualElement.Q<Button>();
            new LocalizedString("Misc", "gotoReceiver").StringChanged += text => button.text = text;
            //var label = ModuleInfo.rootVisualElement.Q<Label>("description");
            //label.text = new string("Sendet das <b>Input</b>-Signal an einen <b>Empfänger</b> weiter.");
        }

        public void Unpair()
        {
            ReceiverModule = null;
            ConnectedModules[_outputPort] = null;
            _unpaired = true;

            var button = ModuleInfo.rootVisualElement.Q<Button>();
            new LocalizedString("Misc", "pair").StringChanged += text => button.text = text;
            //var label = ModuleInfo.rootVisualElement.Q<Label>("description");
            //label.text = new string("Sendet das <b>Input</b>-Signal an einen <b>Empfänger</b> weiter.<br>Mit dem <b>Pair</b>-Button kann ein zugehöriger Empfänger erstellt werden, welcher in der als nächsten ausgewählten Entität platziert werden kann.");
        }
    }
}