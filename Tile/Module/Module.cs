using System;
using System.Collections;
using System.Collections.Generic;
using ElRaccoone.Tweens;
using UnityEngine;
using EPOOutline;
using UnityEngine.UIElements;
using Utility;

namespace Microlayer
{
    /// <summary>
    /// Module is the base class for all objects in the micro layer that partake in the behaviour determination.
    /// These modules are registered in the TileDB and have some meaning for the game play.
    /// </summary>
    public abstract class Module : Tile
    {
        /// <summary>
        /// Defines a side of a module to be not a port, an input port, or an output port.
        /// </summary>
        public enum PortType
        {
            None,
            Input,
            Output,
        }

        /// <summary>
        /// Stores the port definition of the module. 0 is the blue axis. The ports are counted clock-wise.
        /// </summary>
        [Tooltip("Also settable by clicking the rectangles in the scene view.")]
        public PortType[] PortDefinitions = { PortType.None, PortType.None, PortType.None, PortType.None };

        public bool[] PortVisibility = {true,true,true,true};

        /// <summary>
        /// The modules this module is connected to. The index equals the port index.
        /// Check PortDefinitions to check the port type.
        /// </summary>
        [DebugOnly]
        public Module[] ConnectedModules = {null, null, null, null};

        /// <summary>
        /// The modules this module is connected to. The index equals the port index.
        /// Check PortDefinitions to check the port type.
        /// </summary>
        [DebugOnly]
        public Connection[] Connections = {null, null, null, null};

        /// <summary>
        /// Stores all connection port children.
        /// </summary>
        [DebugOnly]
        public List<GameObject> ConnectionPorts = new();

        /// <summary>
        /// Stores all tongues of port children.
        /// </summary>
        [DebugOnly]
        public List<Port> PortTongues = new();

        /// <summary>
        /// Stores all connection port outline toggles.
        /// </summary>
        [DebugOnly]
        public List<Outlinable> PortOutlines = new();

        /// <summary>
        /// Stores all connection port ui handlers.
        /// </summary>
        [DebugOnly]
        public List<HoverUIHandler> PortUIHandlers = new();

        /// <summary>
        /// Stores all connection port ui follow transforms.
        /// </summary>
        [DebugOnly]
        public List<UIFollowTransform> PortUIFollowTransforms = new();

        /// <summary>
        /// Stores module hover ui box.
        /// </summary>
        [DebugOnly]
        public HoverUIHandler ModuleUIHandler;

        /// <summary>
        /// Outline for Module Highlighting.
        /// </summary>
        [HideInInspector]
        public Outlinable Outline;

        public Material StandardMaterial;

        protected UIDocument ModuleInfo;

        private static readonly int ShakeLerpID = Shader.PropertyToID("_ShakeLerp");

        /// <summary>
        /// Returns the direction in world space of the given port. Returns up direction if wrong port is passed.
        /// </summary>
        /// <param name="port">The port to get the direction of. Must be in range 0 ... 3.</param>
        /// <returns>The direction in world space of the port or transform.up if a wrong port was supplied</returns>
        public Vector3 GetDirectionFromPort(int port)
        {
            var xform = transform;
            var forward = xform.forward;
            var right = xform.right;
            Debug.Assert(port is < 4 and >= 0, $"Supplied port index {port} is out of range [0, 3]. transform.up will be returned.", this);
            return port switch
            {
                0 => forward,
                1 => right,
                2 => -forward,
                3 => -right,
                _ => xform.up
            };
        }

        /// <summary>
        /// Returns the cell the port is pointing at
        /// </summary>
        /// <param name="port">[0..4] which side to look at</param>
        /// <returns>of the cell</returns>
        public Vector2Int GetCellFromPort(int port)
        {
            return Grid.WorldToCell(transform.position + GetDirectionFromPort(port) * Grid.CellSize.x);
        }

        /// <summary>
        /// Returns the port, PortType, and direction from a worldPosition.
        /// </summary>
        /// <param name="worldPosition">The position to use to calculate the direction</param>
        /// <returns>The port, PortType, and direction in a tuple</returns>
        public Tuple<int, PortType, Vector3> GetPortFromDirection(Vector3 worldPosition)
        {
            var port = 0;
            var projectedPosition = transform.InverseTransformPoint(worldPosition).normalized;

            // Guess the side
            var xPos = Mathf.Sign(projectedPosition.x);
            if (xPos != 0 && Mathf.Abs(projectedPosition.x) >= Mathf.Abs(projectedPosition.z))
            {
                port = 1;
                if (xPos < 0)
                {
                    port = 3;
                }
            }
            else if (Mathf.Approximately(Mathf.Sign(projectedPosition.z), -1))
            {
                port = 2;
            }

            return new Tuple<int, PortType, Vector3>(port, PortDefinitions[port], GetDirectionFromPort(port));
        }

        public Tuple<int, PortType> GetPortFromCell(Vector2Int cell)
        {
            var (port, type, _) = GetPortFromDirection(Grid.CellToWorld(cell));
            return new Tuple<int, PortType>(port, type);
        }

        /// <summary>
        /// Override to show ui on click. Also override HideUi().
        /// </summary>
        public virtual void ShowUi()
        {
            Debug.Assert(ModuleInfo);
            Debug.Assert(ModuleInfo.rootVisualElement != null);
            ModuleInfo.rootVisualElement.visible = true;
            var dismantle = ModuleInfo.rootVisualElement.Q<Button>("dismantle");
            if (dismantle == null) return;

            dismantle.clicked += () =>
            {
                // Prevents double clicking when module is not yet removed
                if (!Grid.HasTile(Cell)) return;

                Grid.RemoveTile(Cell);
            };
        }

        /// <summary>
        /// Hides the ui when focus of the module was lost. If your ui is hidden earlier, skip the execution.
        /// Prevent focus loss if you wanna keep your window around.
        /// </summary>
        public void HideUi()
        {
            Debug.Assert(ModuleInfo);
            Debug.Assert(ModuleInfo.rootVisualElement != null);
            ModuleInfo.rootVisualElement.visible = false;
        }


        public override void OnAddTile()
        {
            foreach (var module in GetNeighbours<ExecutableModule>())
            {
                if (!module) continue;

                var port = module.GetPortFromDirection(transform.position);
                var ourPort = GetPortFromDirection(module.transform.position);
                if (ourPort.Item2 == PortType.None || port.Item2 == PortType.None || port.Item2 == ourPort.Item2) continue;

                if (this is Connection) continue;
                ConnectedModules[ourPort.Item1] = module;
                module.ConnectedModules[port.Item1] = this;
            }

            SetMaterials(Grid.ColourPalette, Grid.Tiles.ReferencePalette);
            SetupUi();
            UpdatePortPrefabs();
        }

        public override void OnRemoveTile()
        {
            base.OnRemoveTile();
            foreach (var module in ConnectedModules)
            {
                if (!module) continue;

                for (int i = 0; i < 4; i++)
                {
                    if (module.ConnectedModules[i] != this) continue;

                    module.ConnectedModules[i] = null;
                    break;
                }
            }

            if (this is Connection) return;

            foreach (var connection in Connections)
            {
                if (!connection) continue;
                connection.Remove();
            }
        }

        protected virtual void Awake()
        {
            name += Time.frameCount;
            ModuleInfo = transform.GetChild(1).GetComponent<UIDocument>();
            UpdatePortPrefabs();
        }

        protected virtual void Start()
        {
            SetupOutline();
            HideUi();
        }

        public virtual void SetModuleOutline(Color colour, bool visible, Module caller = null)
        {
            if (!Outline) return;

            Outline.OutlineParameters.Color = colour;
            Outline.enabled = visible;
        }

        protected void SetupUi(bool forceAsserts = false)
        {
            if (!ModuleInfo && this is Connection && !forceAsserts) return;
            Debug.Assert(ModuleInfo);

            var entry = Grid.Tiles.GetEntry(GetType());
            var ui = ModuleInfo.rootVisualElement;
            var title = ui.Q<Label>(className: "tiptitle");
            var body = ui.Q<Label>(className: "tiptext");
            Debug.Assert(title != null, this);
            Debug.Assert(body != null, this);
            entry.GetName().StringChanged += theTitle => title.text = theTitle;
            entry.GetModuleInfo().StringChanged += info => body.text = info;

            ModuleUIHandler = GetComponentInChildren<HoverUIHandler>();
            Debug.Assert(ModuleUIHandler);

            ModuleUIHandler.SetAsModule(entry.Group);
            entry.GetName().StringChanged += ModuleUIHandler.SetHoverTitle;
        }

        protected void SetupOutline()
        {
            CreateOutline(ref Outline);
            Outline.OutlineParameters.Color = Color.white;
        }

        private void CreateOutline(ref Outlinable outline)
        {
            if (outline)
            {
                Destroy(outline);
            }

            outline = gameObject.AddComponent<Outlinable>();
            outline.AddAllChildRenderersToRenderingList(RenderersAddingMode.MeshRenderer | RenderersAddingMode.SkinnedMeshRenderer);
            outline.enabled = false;
        }

        protected void UpdatePortPrefabs()
        {
            foreach(GameObject connectionPort in ConnectionPorts)
            {
                Destroy(connectionPort);
            }

            ConnectionPorts.Clear();
            PortTongues.Clear();
            PortOutlines.Clear();
            PortUIHandlers.Clear();
            PortUIFollowTransforms.Clear();

            for (int i = 0; i < PortDefinitions.Length; i++)
            {
                if (PortDefinitions[i] == PortType.None)
                {
                    ConnectionPorts.Add(null);
                    PortTongues.Add(null);
                    PortOutlines.Add(null);
                    PortUIHandlers.Add(null);
                    PortUIFollowTransforms.Add(null);
                    continue;
                }

                if (!PortVisibility[i]) continue;

                var port = Instantiate(SharedModuleData.Instance.PortPrefab,
                    transform.position + SharedModuleData.Instance.PortOffset * GetDirectionFromPort(i) +
                    new Vector3(0, SharedModuleData.Instance.PortHeight, 0),
                    Quaternion.LookRotation(GetDirectionFromPort(i), Vector3.up), transform);

                var portMaterial = PortDefinitions[i] == PortType.Input ? SharedModuleData.Instance.InputMaterial : SharedModuleData.Instance.OutputMaterial;
                port.GetComponentInChildren<MeshRenderer>().material = portMaterial;
                var tongue = port.GetComponentInChildren<Port>();
                tongue.ParentModule = this;

                ConnectionPorts.Add(port);
                PortTongues.Add(tongue);
                PortOutlines.Add(port.transform.GetChild(0).GetComponent<Outlinable>());

                var portUITransform = port.GetComponent<UIFollowTransform>();
                PortUIFollowTransforms.Add(portUITransform);
                var portUI = port.GetComponent<HoverUIHandler>();
                PortUIHandlers.Add(portUI);
                portUI.SetAsInput(PortDefinitions[i] == PortType.Input);

                if (!Grid) continue;

                var entry = Grid.Tiles.GetEntry(GetType());
                Debug.Assert(i < entry.PortTexts.Length);

                if (entry.PortTexts[i].IsEmpty)
                {
                    portUI.SetHoverTitle();
                    continue;
                }
                entry.PortTexts[i].StringChanged += portUI.SetHoverTitle;
            }
        }

        public void SetupOnUnlock()
        {
            ModuleInfo = transform.GetChild(1).GetComponent<UIDocument>();
            UpdatePortPrefabs();
            SetupOutline();
            HideUi();
            SetupUi();
        }

        public IEnumerator ShakeEffect()
        {
            int lerp = 1;
            for(int i = 0; i <= 2; i++)
            {
                foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
                {
                    foreach (Material mat in meshRenderer.materials)
                    {
                        mat.SetFloat(ShakeLerpID, lerp);
                    }
                }

                yield return new WaitForSeconds(0.6f);

                lerp = 0;
            }
        }

        public void PlayPlace()
        {
            StartCoroutine(PlaceEffect());
        }

        private IEnumerator PlaceEffect()
        {
            yield return transform.TweenLocalScale(new Vector3(1.2f, 0.8f, 1.2f), 0.1f).SetEaseQuintOut().Yield();
            yield return transform.TweenLocalScale(new Vector3(0.9f, 1.2f, 0.9f), 0.1f).SetEaseQuintInOut().Yield();
            yield return transform.TweenLocalScale(new Vector3(1f, 1f, 1f), 0.1f);
        }

        public void SetMaterials(ColourPalette palette, ColourPalette reference)
        {
            var remapped = new Dictionary<Material, Material>
            {
                {reference.Actor, palette.Actor},
                {reference.Connection, palette.Connection},
                {reference.Sensor, palette.Sensor},
                {reference.Logic, palette.Logic},
                {reference.LogicDark, palette.LogicDark},
                {reference.LogicLight, palette.LogicLight},
                {reference.LogicGlow, palette.LogicGlow},
            };
            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>(true))
            {
                var materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (!remapped.TryGetValue(materials[i], out var remap)) continue;
                    materials[i] = remap;
                }

                meshRenderer.sharedMaterials = materials;
            }
        }
    }
}
