#if UNITY_EDITOR && false // Remove false to enable execution graph debugging
#define DEBUG_EXECUTION_GRAPH
#endif

using System.Collections.Generic;
using System.Linq;
using Microlayer;
using UnityEngine;
using Utility;

public class ExecutionGraph : MonoBehaviour
{
    private readonly List<ExecutableModule> _executableOrder = new();
    private readonly HashSet<ExecutableModule> _brokenModules = new();
    private readonly HashSet<ExecutableModule> _sinks = new();
    private ExecutableModule _badModule;

    private const float TargetTimeout = 1/10f;
    private float _timer;

#if DEBUG_EXECUTION_GRAPH
    [DebugOnly, SerializeField] private List<ExecutableModule> Sinks;
    [DebugOnly, SerializeField] private List<ExecutableModule> Order;
    [DebugOnly, SerializeField] private List<ExecutableModule> Broken;

#endif

    private void LateUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer < TargetTimeout) return;

        _timer -= TargetTimeout;
        Run();
    }

    public void Build(IEnumerable<ExecutableModule> sources)
    {
        foreach (var module in _executableOrder)
        {
            module.OnGraphRebuildRequested();
        }

        _brokenModules.Clear();
        _executableOrder.Clear();
        _sinks.Clear();
        foreach (var source in sources)
        {
            FindSinks(source, new HashSet<ExecutableModule>(), new HashSet<ExecutableModule>());
        }
#if DEBUG_EXECUTION_GRAPH
        Sinks = _sinks.ToList();
#endif
        var allModules = new HashSet<ExecutableModule>();
        foreach (var module in _sinks)
        {
            _badModule = null;
            BuildExecutionOrder(module, new HashSet<ExecutableModule>(), allModules);
        }
#if DEBUG_EXECUTION_GRAPH
        Broken = _brokenModules.ToList();
        Order = _executableOrder.ToList();
#endif
        foreach (var module in _executableOrder)
        {
            module.OnGraphBuilt();
        }
    }

    private void Run()
    {
        foreach (var module in _executableOrder)
        {
            module.ClearInput();
        }
        foreach (var module in _executableOrder)
        {
            if (_brokenModules.Contains(module))
            {
                module.GarbageExecute();
            }
            else
            {
                module.Execute();
            }
        }
    }

    private void FindSinks(ExecutableModule module, ISet<ExecutableModule> addedModules, ISet<ExecutableModule> allModules)
    {
        if (addedModules.Contains(module)) return;

        if (allModules.Contains(module))
        {
            // We encountered a splitter that's already added to the graph, so we can rely on the previous execution.
            // We still add it to our addedModules in case here's a recursion.
            addedModules.Add(module);
            return;
        }

        addedModules.Add(module);
        allModules.Add(module);
        var isSink = true;
        for (var i = 0; i < 4; i++)
        {
            if (module.PortDefinitions[i] != Module.PortType.Output) continue;

            var connectedModule = module.ConnectedModules[i];
            if (!connectedModule || connectedModule is not ExecutableModule executableModule)
            {
                Debug.Assert(!connectedModule, "There is something connected, that shouldn't be.", module);
                // ¯\_(ツ)_/¯
                continue;
            }

            FindSinks(executableModule, new HashSet<ExecutableModule>(addedModules), allModules);
            isSink = false;
        }

        if (isSink)
        {
            _sinks.Add(module);
        }
    }

    private bool BuildExecutionOrder(ExecutableModule module, ISet<ExecutableModule> addedModules, ISet<ExecutableModule> allModules)
    {
        if (addedModules.Contains(module))
        {
            // Detects local recursion.
            _brokenModules.Add(module);
            _executableOrder.Add(module);
            _badModule = module;
            return false;
        }

        if (allModules.Contains(module))
        {
            // We encountered a splitter that's already added to the graph, so we can rely on the previous execution.
            // We still add it to our addedModules in case here's a recursion.
            addedModules.Add(module);
            return true;
        }

        addedModules.Add(module);
        allModules.Add(module);
        var recursed = false;
        var inputModuleCount = 0;
        for (var i = 0; i < module.ConnectedModules.Length; i++)
        {
            if (module.PortDefinitions[i] != Module.PortType.Input) continue;

            var connectedModule = module.ConnectedModules[i];
            if (connectedModule == null || connectedModule is not ExecutableModule executableModule)
            {
                // ¯\_(ツ)_/¯
                continue;
            }

            recursed |= !BuildExecutionOrder(executableModule, new HashSet<ExecutableModule>(addedModules), allModules);
            inputModuleCount++;
        }

        // Order matters here!
        // We don't care about recursion unless it would cause issues.
        // Rather, we want to strip dead code and recursions.

        if (module.PortDefinitions.Count(type => type == Module.PortType.Input) != inputModuleCount)
        {
            _brokenModules.Add(module);
        }

        if (recursed && module == _badModule)
        {
            // Was already added
            return true; // We need to return false until we reached the non recursed module.
        }

        _executableOrder.Add(module);
        return !recursed;
    }
}