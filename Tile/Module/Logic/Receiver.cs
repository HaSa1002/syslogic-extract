using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Microlayer
{
    public class Receiver : Logic
    {
        public Transmitter TransmitterModule;
        private int _inputPort = -1;
        private bool _paired = true;

        public Brain Brain;

        private BrainControl _brainControl;

        private bool _firstRun = true;

        public override void OnAddTile()
        {
            base.OnAddTile();

            Brain = GetComponentInParent<Brain>();
            _brainControl = GetComponentInParent<BrainControl>();

            for (var i = 0; i < 4; i++)
            {
                if (PortDefinitions[i] == PortType.Input)
                {
                    _inputPort = i;
                }
            }
            ConnectedModules[_inputPort] = TransmitterModule;
            TransmitterModule.AssignReceiver(this);
            _brainControl.FinishPairMode();
        }

        public override void OnRemoveTile()
        {
            base.OnRemoveTile();
            if (!_paired) return;

            TransmitterModule.Unpair();
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
            new LocalizedString("Misc", "gotoTransmitter").StringChanged += text => button.text = text;
            button.clicked += OnChangeValue;
        }

        private void OnChangeValue()
        {
            if (!_paired) return;

            TransmitterModule.GetComponentInParent<Brain>().Show();
            HideUi();
        }

        public void Unpair()
        {
            TransmitterModule = null;
            ConnectedModules[_inputPort] = null;
            _paired = false;

            var button = ModuleInfo.rootVisualElement.Q<Button>();
            new LocalizedString("Misc", "unpaired").StringChanged += text => button.text = text;
        }
    }
}