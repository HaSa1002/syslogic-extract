using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Microlayer
{
    /// <summary>
    /// Base class for all execution graph relevant modules.
    /// Descendants of this class drive the simulation.
    /// </summary>
    public abstract class ExecutableModule : Module
    {

        /// <summary>
        /// Data to use in execute
        /// </summary>
        protected readonly List<MicroData> Input = new();

        public Material GlowMaterial;
        [HideInInspector]
        public Material _glowMat;

        private static readonly int Running = Animator.StringToHash("running");
        private static readonly int EmissionValue = Shader.PropertyToID("_EmissionValue");

        /// <summary>
        /// Behaviour of the module.
        /// </summary>
        /// <returns>Array of the output.</returns>
        public abstract void Execute();


        /// <summary>
        /// Broken/Undefined behaviour of the module. Called if found inside a recursion.
        /// Override this method for more specific broken/garbage behaviour.
        /// Default is returning random values.
        /// </summary>
        /// <returns>Array with output.</returns>
        public virtual void GarbageExecute()
        {
            var result = new List<MicroData>();
            for (int i = 0; i < PortDefinitions.Count(type => type == PortType.Output); ++i)
            {
                result.Add(Random.value);
            }

            PushData(result);
        }

        /// <summary>
        /// Called once the graph building is done and the module is going to run.
        /// </summary>
        public virtual void OnGraphBuilt()
        {
            UpdateVisualState(true);
        }

        /// <summary>
        /// Called when the graph is about to be rebuild and the module was running.
        /// </summary>
        public virtual void OnGraphRebuildRequested()
        {
            if (!this) return;
            if (Input.Count > 0)
            {
                Input[0] = 0;
                UpdateEmissionValue((Input[0]));
            }
            UpdateVisualState(false);
            ResetConnections();
        }


        /// <summary>
        /// States whether or not this module can execute standalone or needs input to be meaningful.
        /// Used for dead code elimination.
        /// </summary>
        /// <returns>Returns true if standalone executable.</returns>
        public virtual bool IsExecutionSource()
        {
            return false;
        }

        /// <summary>
        /// Updates the next modules with the output of this method.
        /// NOTE: This method MUST be CALLED in execute (if you have output ports).
        /// Failing to oblige results in a CRASH in the NEXT module.
        /// </summary>
        /// <param name="output">A list or array of the output</param>
        protected void PushData(IReadOnlyList<MicroData> output)
        {
            var outputIndex = 0;
            for (var i = 0; i < 4; i++)
            {
                if (PortDefinitions[i] != PortType.Output) continue;
                if (ConnectedModules[i] is not ExecutableModule executableModule) continue;

                Debug.Assert(outputIndex < output.Count);
                executableModule.Input.Add(output[outputIndex]);
                if (Connections[i])
                {
                    Connections[i].UpdateVisualState(output[outputIndex]);
                }
                outputIndex++;
            }
        }

        /// <summary>
        /// Clears the input buffer of this module.
        /// </summary>
        public void ClearInput()
        {
            Input.Clear();
        }

        /// <summary>
        /// Updates the visual state of the module depending on the connection state.
        /// Can be customised for connected state updates. Current value updates need to be done
        /// manually in the execute method.
        /// </summary>
        /// <param name="running"> states the connection state</param>
        protected virtual void UpdateVisualState(bool running)
        {
            var animator = GetComponentInChildren<Animator>();

            if (animator)
            {
                animator.SetBool(Running, running);
            }
        }

        protected virtual void SetEmissionValue(Material m, float value)
        {
            m.SetFloat(EmissionValue, value);
        }

        protected void UpdateEmissionValue(List<MicroData> values)
        {
            var max = 0f;
            if (values.Count > 0)
            {
                max = (float)values[0];
                foreach (var v in values)
                {
                    max = Mathf.Max((float)v, max);
                }
            }
            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                var materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                {
                    if (meshRenderer.sharedMaterials[i] == _glowMat)
                    {
                        SetEmissionValue(materials[i], max);
                    }
                }
                meshRenderer.sharedMaterials = materials;
            }
        }

        protected void UpdateEmissionValue(MicroData[] values)
        {
            var max = (float)values[0];
            foreach (var v in values)
            {
                max = Mathf.Max((float)v, max);
            }
            foreach (var meshRenderer in this.GetComponentsInChildren<MeshRenderer>())
            {
                var materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                {
                    if (meshRenderer.sharedMaterials[i] == _glowMat)
                    {
                        SetEmissionValue(materials[i], max);
                    }
                }
                meshRenderer.sharedMaterials = materials;
            }
        }

        protected void UpdateEmissionValue(MicroData value)
        {
            foreach (var meshRenderer in this.GetComponentsInChildren<MeshRenderer>())
            {
                var materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                {
                    if (meshRenderer.sharedMaterials[i] == _glowMat)
                    {
                        SetEmissionValue(materials[i], (float)value);
                    }
                }
                meshRenderer.sharedMaterials = materials;
            }
        }

        protected void UpdateEmissionValue(float value, Material material)
        {
            SetEmissionValue(material, value);
        }

        protected void ResetConnections()
        {
            for (var i = 0; i < 4; i++)
            {
                if (Connections[i])
                {
                    Connections[i].UpdateVisualState(0);
                }
            }
        }

        protected void SetGlowMaterial()
        {
            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                var materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                {
                    if (meshRenderer.sharedMaterials[i] == GlowMaterial)
                    {
                        materials[i] = _glowMat;
                    }
                }
                meshRenderer.sharedMaterials = materials;
            }
        }
    }
}
