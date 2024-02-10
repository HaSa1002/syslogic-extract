using Deform;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;

namespace Microlayer
{
    public class Connection : Module
    {
        private class ConnectionType
        {
            public Mesh MeshType;
            public int RotationAngle;
        }

        [DebugOnly]
        public Vector2Int PreviousModule = new(int.MaxValue, int.MaxValue);
        [DebugOnly]
        public Vector2Int NextModule = new(int.MaxValue, int.MaxValue);

        [DebugOnly]
        public bool MarkedForDelete;

        [SerializeField]
        private Mesh DefaultMesh, CornerMesh, CornerMeshInvert, DefaultMeshUp, DefaultMeshUpInvert, CornerMeshUp, CornerMeshUpInvert;

        private Vector3Int _inputPortDirection;

        private Dictionary<Vector3Int, ConnectionType> LongType1, LongType2, CornerType3, CornerType4, CornerType5, CornerType6;

        private Transform _meshChild;
        private MeshFilter _meshFilter;
        private MeshRenderer _renderer;
        private Material _valueMaterial;
        private static readonly int DataValue = Shader.PropertyToID("_DataValue");

        private Deformable _deformable;

        [SerializeField] private GameObject CheckpointPrefab;
        private GameObject _checkpointPreview;

        [DebugOnly] private int _visualType;

        private float _halfCellSize;
        private Transform _blobxform;
        private SpherifyDeformer _blob;
        private float _blobSpeed;
        private bool _isReverse;
        private float _isMirrored = 1;
        [DebugOnly] public int ConnectionLength;
        [DebugOnly] public int ConnectionIndex;
        private bool _signalReversed;

        private bool _isStartTile;
        private bool _isTemporary;

        [SerializeField]
        private VisualTreeAsset ModuleInfoVisualTreeAsset;

        [SerializeField] private PanelSettings ModuleInfoPanelSettings;

        [SerializeField] private GameObject HoverPrefab;

        protected override void Awake()
        {
            _isTemporary = true;
            // The default values are not applied because Unity serialised some value and our debug only doesn't prevent serialisation.
            PreviousModule = new Vector2Int(int.MaxValue, int.MaxValue);
            NextModule = new Vector2Int(int.MaxValue, int.MaxValue);
            _meshChild = transform.GetChild(0);
            _meshFilter = _meshChild.GetComponent<MeshFilter>();
            _renderer = _meshChild.GetComponent<MeshRenderer>();
            _deformable = GetComponentInChildren<Deformable>();
            _blobxform = _meshChild.GetChild(0);
            _blob = _blobxform.GetComponent<SpherifyDeformer>();
            Debug.Assert(_meshChild);
            Debug.Assert(_meshFilter);
            Debug.Assert(_renderer);
            Debug.Assert(_deformable);
            Debug.Assert(_blobxform);
            Debug.Assert(_blob);

            LongType1 = new Dictionary<Vector3Int, ConnectionType>()
            {
             { new Vector3Int(0, 0, 1), new ConnectionType() { RotationAngle = 0, MeshType = DefaultMeshUpInvert } },
             { new Vector3Int(0, 0, -1), new ConnectionType() { RotationAngle = 0, MeshType = DefaultMeshUp } }
            };

            LongType2 = new Dictionary<Vector3Int, ConnectionType>()
            {
             { new Vector3Int(1, 0, 0), new ConnectionType() { RotationAngle = 0, MeshType = DefaultMeshUp } },
             { new Vector3Int(-1, 0, 0), new ConnectionType() { RotationAngle = 0, MeshType = DefaultMeshUpInvert } }
            };

            CornerType3 = new Dictionary<Vector3Int, ConnectionType>()
            {
             { new Vector3Int(1, 0, 0), new ConnectionType() { RotationAngle = -90, MeshType = CornerMeshUpInvert } },
             { new Vector3Int(0, 0, -1), new ConnectionType() { RotationAngle = 0, MeshType = CornerMeshUp } }
            };

            CornerType4 = new Dictionary<Vector3Int, ConnectionType>()
            {
             { new Vector3Int(0, 0, -1), new ConnectionType() { RotationAngle = 0, MeshType = CornerMeshUpInvert } },
             { new Vector3Int(-1, 0, 0), new ConnectionType() { RotationAngle = 90, MeshType = CornerMeshUp } }
            };

            CornerType5 = new Dictionary<Vector3Int, ConnectionType>()
            {
             { new Vector3Int(0, 0, 1), new ConnectionType() { RotationAngle = 180, MeshType = CornerMeshUp } },
             { new Vector3Int(-1, 0, 0), new ConnectionType() { RotationAngle = 90, MeshType = CornerMeshUpInvert } }
            };


            CornerType6 = new Dictionary<Vector3Int, ConnectionType>()
            {
             { new Vector3Int(1, 0, 0), new ConnectionType() { RotationAngle = 270, MeshType = CornerMeshUp } },
             { new Vector3Int(0, 0, 1), new ConnectionType() { RotationAngle = 180, MeshType = CornerMeshUpInvert } }
            };
        }

        // Don't show ports on connections.
        protected override void Start() { }

        public override void OnAddTile()
        {
            base.OnAddTile();
            _halfCellSize = Grid.CellSize.x / 2f;

            _valueMaterial = Instantiate(_renderer.material);
            _renderer.material = _valueMaterial;
        }

        public void OnBecamePermanent()
        {
            ModuleInfo = _meshChild.gameObject.AddComponent<UIDocument>();
            ModuleInfo.sortingOrder = 2;
            ModuleInfo.panelSettings = ModuleInfoPanelSettings;
            ModuleInfo.visualTreeAsset = ModuleInfoVisualTreeAsset;
            _meshChild.gameObject.AddComponent<WorldspaceUi>().ScreenOffset = new Vector2(0, 100);
            Instantiate(HoverPrefab, transform);
            _isTemporary = false;
            SetupUi(true);
            HideUi();
        }

        public override void OnRemoveTile()
        {
            if (MarkedForDelete) return;

            Destroy(gameObject);
            MarkedForDelete = true;

            var previous = Grid.GetTile<Module>(PreviousModule);
            if (previous is Connection { MarkedForDelete: false })
            {
                Grid.RemoveTile(PreviousModule);
            }
            else
            {
                UpdateModuleOnDismantle(previous);
            }

            var next = Grid.GetTile<Module>(NextModule);
            if (next is Connection  {MarkedForDelete: false})
            {
                Grid.RemoveTile(NextModule);
            }
            else
            {
                UpdateModuleOnDismantle(next);
            }
        }

        /// <summary>
        /// Sets the outline colour and visibility on the whole connection. Pass the connection itself as caller
        /// if you wish to only set the colour on one connection.
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="visible"></param>
        /// <param name="caller"></param>
        public override void SetModuleOutline(Color colour, bool visible, Module caller = null)
        {
            base.SetModuleOutline(colour, visible, caller);
            if (caller == this) return;

            var previous = Grid.GetTile<Connection>(PreviousModule);
            if (previous && previous != caller)
            {
                previous.SetModuleOutline(colour, visible, this);
            }

            var next = Grid.GetTile<Connection>(NextModule);
            if (next && next != caller)
            {
                next.SetModuleOutline(colour, visible, this);
            }
        }

        /// <summary>
        /// Only deletes the single tile
        /// </summary>
        public void RemoveSingle()
        {
            MarkedForDelete = true;
            Grid.RemoveTile(Cell);
            Destroy(gameObject);
            UpdateModuleOnDismantle(Grid.GetTile<Module>(PreviousModule));
            UpdateModuleOnDismantle(Grid.GetTile<Module>(NextModule));
        }

        private void UpdateModuleOnDismantle(Module module)
        {
            if (!module) return;

            var port = module.GetPortFromCell(Cell).Item1;
            module.Connections[port] = null;
            module.ConnectedModules[port] = null;
        }

        public void UpdateVisualState(MicroData value)
        {
            if (_isTemporary) return;

            if (_blob)
            {
                _blob.Radius = (float)value + 0.7f;
                _blobSpeed = (float)value * 10f * ConnectionLength;
            }
            _valueMaterial.SetFloat(DataValue, (float)value);
            var next = Grid.GetTile<Connection>(NextModule);
            if (next)
            {
                next.UpdateVisualState(value);
            }
        }

        public void ChooseConnection(bool isStartTile, bool preview, Vector3 portDirection)
        {
            _isStartTile = isStartTile;

            _inputPortDirection = Vector3Int.RoundToInt(portDirection);

            if(preview && !isStartTile)
            {
                _meshChild.transform.localPosition = new Vector3(0, 1.979F, 0);
            }
            else if (!preview && !isStartTile)
            {
                _meshChild.transform.localPosition = new Vector3(0, 0, 0);
            }

            _meshChild.localRotation = Quaternion.identity;
            if (_checkpointPreview)
            {
                Destroy(_checkpointPreview);
            }
            _renderer.enabled = true;
            if (PortDefinitions[0] != PortType.None && PortDefinitions[2] != PortType.None)
            {
                _visualType = 1;
            }
            else if (PortDefinitions[1] != PortType.None && PortDefinitions[3] != PortType.None)
            {
                _visualType = 2;
            }
            else if (PortDefinitions[0] != PortType.None && PortDefinitions[3] != PortType.None)
            {
                _visualType = 3;
            }
            else if (PortDefinitions[0] != PortType.None && PortDefinitions[1] != PortType.None)
            {
                _visualType = 4;
            }
            else if (PortDefinitions[1] != PortType.None && PortDefinitions[2] != PortType.None)
            {
                _visualType = 5;
            }
            else if (PortDefinitions[3] != PortType.None && PortDefinitions[2] != PortType.None)
            {
                _visualType = 6;
            }

            if (_isStartTile)
            {
                SetPreviewVisual();
                return;
            }

            SetVisual();
        }

        public void SetPreviewVisual()
        {

            BlobDirection();

            switch (_visualType)
            {
                case 0:
                    break;
                case 1:
                    _meshFilter.mesh = LongType1[_inputPortDirection].MeshType;
                    break;
                case 2:
                    _meshChild.Rotate(Vector3.up, -90);
                    _meshFilter.mesh = LongType2[_inputPortDirection].MeshType;
                    break;
                case 3:
                    _meshChild.Rotate(Vector3.up, CornerType3[_inputPortDirection].RotationAngle);
                    _meshFilter.mesh = CornerType3[_inputPortDirection].MeshType;
                    break;
                case 4:
                    _meshChild.Rotate(Vector3.up, CornerType4[_inputPortDirection].RotationAngle);
                    _meshFilter.mesh = CornerType4[_inputPortDirection].MeshType;
                    break;
                case 5:
                    _meshChild.Rotate(Vector3.up, CornerType5[_inputPortDirection].RotationAngle);
                    _meshFilter.mesh = CornerType5[_inputPortDirection].MeshType;
                    break;
                case 6:
                    _meshChild.Rotate(Vector3.up, CornerType6[_inputPortDirection].RotationAngle);
                    _meshFilter.mesh = CornerType6[_inputPortDirection].MeshType;
                    break;
            }

            SetupOutline();
        }

        public void SetVisual()
        {
            BlobDirection();

            switch(_visualType)
            {
                case 0:
                    break;
                case 1:
                    _meshFilter.mesh = DefaultMesh;
                    break;
                case 2:
                    //Default Mesh
                    //90 degree rotation
                    _meshChild.Rotate(Vector3.up, -90);
                    _meshFilter.mesh = DefaultMesh;
                    break;
                case 3:
                    _meshFilter.mesh = CornerMesh;
                    break;
                case 4:
                    //Corner Mesh
                    _meshFilter.mesh = CornerMeshInvert;
                    break;
                case 5:
                    //Corner Mesh
                    //180 degree rotation
                    _meshChild.Rotate(Vector3.up, 180);
                    _meshFilter.mesh = CornerMesh;
                    break;
                case 6:
                    //Corner Mesh
                    //270 degree rotation
                    _meshChild.Rotate(Vector3.up, 270);
                    _meshFilter.mesh = CornerMesh;
                    break;
            }
            SetupOutline();
        }

        private void BlobDirection()
        {
            foreach (var p in PortDefinitions) //blob direction magic...
            {
                if (p == PortType.Input && (_visualType == 2 || _visualType == 5 || _visualType == 6))
                {
                    _isReverse = true;
                    break;
                }
                if (p == PortType.Input && (_visualType == 1 || _visualType == 3))
                {
                    _isReverse = false;
                    break;
                }
                if (p == PortType.Output && (_visualType == 2 || _visualType == 6 || _visualType == 5))
                {
                    _isReverse = false;
                    break;
                }
                if (p == PortType.Output && (_visualType == 1 || _visualType == 3))
                {
                    _isReverse = true;
                    break;
                }
                if (p == PortType.Input && _visualType == 4)
                {
                    _isMirrored = -1;
                    _isReverse = false;
                    break;
                }
                if (p == PortType.Output && _visualType == 4)
                {
                    _isMirrored = -1;
                    _isReverse = true;
                    break;
                }
            }
        }

        public void PreviewAsCheckpoint(bool hideUi = true)
        {
            _renderer.enabled = true;
            if (_checkpointPreview)
            {
                Destroy(_checkpointPreview);
            }
            _checkpointPreview = Instantiate(CheckpointPrefab, transform);
            _checkpointPreview.SetActive(true);

            SetupOutline();
        }

        public void HidePreview()
        {
            _renderer.enabled = false;
            if (_checkpointPreview)
            {
                Destroy(_checkpointPreview);
            }
        }

        /// <summary>
        /// Replaces this connection with a connection checkpoint while maintaining all relevant data.
        /// </summary>
        public Splitter ReplaceWithCheckpoint(bool copyDefinitions = true)
        {
            Module input = null;
            Module output = null;
            for(var i = 0; i < 4; i++)
            {
                if (!ConnectedModules[i]) continue;
                Debug.Assert(ConnectedModules[i] is not Connection);
                switch (PortDefinitions[i])
                {
                    case PortType.Input:
                        input = ConnectedModules[i];
                        break;
                    case PortType.Output:
                        output = ConnectedModules[i];
                        break;
                    case PortType.None:
                    default:
                        break;
                }
            }

            int inputPort = -1;
            int outputPort = -1;
            for (int i = 0; i < 4; i++)
            {
                if (input && input.ConnectedModules[i] == output)
                {
                    inputPort = i;
                }

                if (output && output.ConnectedModules[i] == input)
                {
                    outputPort = i;
                }
            }

            RemoveSingle();

            var checkpoint = Grid.AddTile<Splitter>(Cell, true);
            for(var i = 0; i < ConnectedModules.Length; i++)
            {
                if (copyDefinitions)
                {
                    checkpoint.PortDefinitions[i] = PortType.Output;
                }
                Debug.Assert(ConnectedModules[i] is not Connection);
                checkpoint.ConnectedModules[i] = ConnectedModules[i];
                if (PortDefinitions[i] == PortType.None) continue;
                if (copyDefinitions)
                {
                    checkpoint.PortDefinitions[i] = PortDefinitions[i];
                }
            }

            checkpoint.Connections[checkpoint.GetPortFromCell(PreviousModule).Item1] =
                Grid.GetTile<Connection>(PreviousModule);
            var nextModule = checkpoint.Connections[checkpoint.GetPortFromCell(NextModule).Item1] = Grid.GetTile<Connection>(NextModule);


            if (input)
            {
                input.ConnectedModules[inputPort] = checkpoint;
                for (var connection = input.Connections[inputPort];
                     connection;
                     connection = Grid.GetTile<Connection>(connection.NextModule))
                {
                    connection.ConnectedModules[connection.GetPortFromCell(connection.NextModule).Item1] = checkpoint;
                    connection.ConnectionLength = ConnectionIndex;
                }
            }

            if (output)
            {
                output.ConnectedModules[outputPort] = checkpoint;
                for (var connection = output.Connections[outputPort];
                     connection;
                     connection = Grid.GetTile<Connection>(connection.PreviousModule))
                {
                    connection.ConnectedModules[connection.GetPortFromCell(connection.PreviousModule).Item1] = checkpoint;
                    connection.ConnectionLength -= ConnectionIndex + 1;
                    connection.ConnectionIndex -= ConnectionIndex + 1;
                }
            }
            checkpoint.OnAddTile();
            if (nextModule)
            {
                nextModule.StartCoroutine(nextModule.StartBlob());
            }
            Grid.ConvertTemporaryTiles();
            return checkpoint;
        }

        private IEnumerator Blob(bool corner, bool reverse)
        {
                _deformable.DeformerElements[0].Active = true;

                var pos = reverse ? -_halfCellSize : _halfCellSize;
                var endPos = reverse ? _halfCellSize : -_halfCellSize;
                var rot = 0f;
                if (corner)
                {
                    rot = reverse ? 90f : 0f;
                    _blobxform.localRotation = Quaternion.Euler(0, rot, 0);
                }

                while (reverse ? pos < endPos : pos > endPos)
                {
                    if (!_blobxform) yield break;

                    if (reverse ? pos + Time.deltaTime * _blobSpeed >= endPos : pos - Time.deltaTime * _blobSpeed <= endPos)
                    {
                        if (corner)
                        {
                            rot = reverse ? 90f : 0f;
                            _blobxform.localRotation = Quaternion.Euler(0, rot, 0);
                        }
                        //ended this tile, can start next tile
                        _deformable.DeformerElements[0].Active = false;

                        var c = Grid.GetTile<Connection>(NextModule);
                        if (c)
                        {
                            yield return c.AttachBlob();
                        }
                        break;
                    }
                    pos = reverse ? Mathf.Min(endPos, pos + Time.deltaTime * _blobSpeed) : Mathf.Max(endPos, pos - Time.deltaTime * _blobSpeed);
                    if (corner && pos >= -0.75f && pos <= 0.75f)
                    {
                        rot = reverse ? Mathf.Max(0f, rot - Time.deltaTime * (_blobSpeed * 60f)) : Mathf.Min(90f, rot + Time.deltaTime * (_blobSpeed * 60f));
                        _blobxform.localRotation = Quaternion.Euler(0, rot, 0);
                    }
                    if (corner && (reverse ? pos >= 0f : pos <= 0f))
                    {
                        _blobxform.localPosition = reverse ? new Vector3(0f, 0.5f, pos) : new Vector3(_isMirrored*pos, 0.5f, 0f);
                    }
                    else if (corner)
                    {
                        _blobxform.localPosition = reverse ? new Vector3( _isMirrored*pos, 0.5f, 0f) : new Vector3(0f, 0.5f, pos);
                    }
                    else
                    {
                        _blobxform.localPosition = new Vector3(0f, 0.5f, pos);
                    }
                    yield return null;
                }
        }

        public IEnumerator AttachBlob()
        {
            if (_visualType >= 3)
            {
                _deformable.DeformerElements[2].Active = true;
                yield return Blob(true, _isReverse);
            }
            else
            {
                _deformable.DeformerElements[1].Active = true;
                yield return Blob(false, _isReverse);
            }
        }

        public IEnumerator StartBlob()
        {
            while (true)
            {
                yield return AttachBlob();
            }
        }
    }
}
