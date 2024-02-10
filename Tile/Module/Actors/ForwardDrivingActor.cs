using UnityEngine;
using UnityEngine.Serialization;

namespace Microlayer
{
    public class ForwardDrivingActor : Actor
    {
        [FormerlySerializedAs("speed")] public float Speed = 5;
        private MicroData _speedFactor;
        private Rigidbody _rb;

        public override void OnGraphBuilt()
        {
            base.OnGraphBuilt();
            _rb = Target.GetComponent<Rigidbody>();
        }

        public override void OnGraphRebuildRequested()
        {
            base.OnGraphRebuildRequested();
            _speedFactor = 0;
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            if (!Mathf.Approximately((float)_speedFactor, (float)Input[0]))
            {
                EmitProgressionValue();
            }
            _speedFactor = Input[0];
        }

        private void FixedUpdate()
        {
            if (!_rb)
            {
                return;
            }
            _rb.position += Target.forward * (Speed * (float)_speedFactor * Time.fixedDeltaTime);
        }
    }
}