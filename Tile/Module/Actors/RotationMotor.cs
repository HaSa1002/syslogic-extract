using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Microlayer
{
    public class RotationMotor : Actor
    {
        [FormerlySerializedAs("rotationSpeed")] public float RotationSpeed = -27.5f;
        private MicroData _rotate;
        private MicroData _clockwise;
        private int _garbageCount;

        public override void GarbageExecute()
        {
            for (int i = 0; i <= 2 - Input.Count; i++)
            {
                _garbageCount++;
                Input.Add(Random.value);
            }
            Execute();
        }

        public override void OnGraphRebuildRequested()
        {
            base.OnGraphRebuildRequested();
            _rotate = 0;
            _clockwise = 0;
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            if (_garbageCount == 0)
            {
                EmitProgressionValue(Input[1]);
            }
            _rotate = Input[0];
            _clockwise = Input[1];
            _garbageCount = 0;
        }

        private void FixedUpdate()
        {
            Target.Rotate(Target.up, (float)_rotate * RotationSpeed * Time.fixedDeltaTime * (_clockwise ? -1.0f : 1.0f));
        }
    }
}