using UnityEngine;

namespace Microlayer
{
    public class ObstacleSensor : Sensor
    {
        private const float MaxDistance = 10;
        [SerializeField] private float SpherecastRadius = 0.3f;

        /// <summary>
        /// Returns the distance from any collidable object in view direction.
        /// </summary>
        /// <returns>The distance from any collidable object in view direction.</returns>
        public override void Execute()
        {
            var distance = float.PositiveInfinity;
            if (Physics.SphereCast(MacroSensorPosition.position, SpherecastRadius,MacroSensorPosition.forward, out var raycastHit, MaxDistance))
            {
                distance = raycastHit.distance;

            }
            var output = new MicroData[] { Mathf.Min(distance, MaxDistance) / MaxDistance };
            PushData(output);
            UpdateEmissionValue(output);
        }
    }
}