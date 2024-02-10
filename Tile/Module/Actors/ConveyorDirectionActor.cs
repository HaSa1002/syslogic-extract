using Macrolayer;
using UnityEngine;

namespace Microlayer
{
    public class ConveyorDirectionActor : Actor
    {
        private Conveyor _conveyor;
        
        protected override void Start()
        {
            base.Start();
            _conveyor = Target.GetComponent<Conveyor>();
            Debug.Assert(_conveyor, "The target needs to be a conveyor.", this);
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            _conveyor.Direction = (bool)Input[0] ? 1 : -1;
        }
    }
}