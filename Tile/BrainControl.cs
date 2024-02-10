using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microlayer
{
    [RequireComponent(typeof(ExecutionGraph))]
    public class BrainControl : MonoBehaviour
    {
        public Brain[] Brains;
        private readonly HashSet<Type> _unlockedTiles = new();
        private bool _unlockNotifyRequested;

        [SerializeField] private BuildUIEventHandler BuildUi;

        [SerializeField] private PictureInPicture PicInPic;

        private bool _executionSourcesChanged;
        private ExecutionGraph _executionGraph;
        private PlaceModule _placeModule;

        private bool _pairMode;
        private Transmitter _currentPairTransmitter;
        private Receiver _currentPairReceiver;

        private void Awake()
        {
            Brains = GetComponentsInChildren<Brain>(true);
            _executionGraph = GetComponent<ExecutionGraph>();
            _placeModule = GetComponentInChildren<PlaceModule>();
        }

        private void Start()
        {
            foreach (var brain in Brains)
            {
                brain.Hide();
                brain.GetComponentInChildren<GridMap>().ExecutionSourcesChanged +=
                    () => _executionSourcesChanged = true;
            }
        }

        private void LateUpdate()
        {
            if (!_executionSourcesChanged) return;

            var sources = new List<ExecutableModule>();
            foreach (var brain in Brains)
            {
                sources.AddRange(brain.GetComponentInChildren<GridMap>().ExecutionSources);
            }
            _executionGraph.Build(sources);
            _executionSourcesChanged = false;
        }

        public void RequestFocus(Brain requester)
        {
            foreach (var brain in Brains)
            {
                if (brain == requester) continue;
                brain.Hide();
            }

            var grid = requester.GetComponentInChildren<GridMap>();
            var cam = GetComponentInChildren<CameraLookTargetMicro>();
            cam.Grid = grid;

            if (PicInPic.IsInWorld)
            {
                SwitchLayer();
            }

            cam.JumpToTile(new Vector3(0, 0, 0));

            _placeModule.Grid = grid;

            if (!_pairMode || requester == _currentPairTransmitter.GetComponentInParent<Brain>()) return;

            _currentPairReceiver = _placeModule.SpawnModule(_placeModule.Tiles.GetTile<Receiver>()) as Receiver;
            _currentPairReceiver!.TransmitterModule = _currentPairTransmitter;
        }

        public void ActivateEntity(Brain brain)
        {
            brain.SetActive(true);
            brain.Target.gameObject.SetActive(true);
        }

        public void Unlock(Type type)
        {
            Debug.Assert(!_unlockedTiles.Contains(type),
                $"Tile {type} was already unlocked. This is assumed to be a bug. Search offensive programming.", this);

            StartCoroutine(UnlockTile(type));
        }

        public void EnterPairMode(Transmitter transmitter)
        {
            _currentPairTransmitter = transmitter;
            _pairMode = true;
            _placeModule.PostProcessingBorderEffect(false, _pairMode);

            var cam = GetComponentInChildren<CameraLookTargetMicro>();
            SwitchLayer();
        }

        public void SwitchLayer()
        {
            StartCoroutine(PicInPic.SwitchLayerEffect());
        }

        public void FinishPairMode()
        {
            _currentPairTransmitter = null;
            _currentPairReceiver = null;
            _pairMode = false;
            _placeModule.PostProcessingBorderEffect(false, _pairMode);
        }

        private IEnumerator UnlockTile(Type type)
        {
            _unlockedTiles.Add(type);
            if (_unlockNotifyRequested) yield break;

            _unlockNotifyRequested = true;
            yield return null;

            BuildUi.UpdateUi(_unlockedTiles);
            _unlockNotifyRequested = false;
        }
    }
}
