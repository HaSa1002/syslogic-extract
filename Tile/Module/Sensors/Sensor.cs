using UnityEngine;

namespace Microlayer
{
    public abstract class Sensor : ExecutableModule
    {
        /// <summary>
        /// Position in the macro layer where the sensor is sitting.
        /// In its simplest form this is the entity transform.
        /// </summary>
        [HideInInspector]
        public Transform MacroSensorPosition;

        public override void OnAddTile()
        {
            base.OnAddTile();

            _glowMat = Instantiate(GlowMaterial);

            SetGlowMaterial();
        }

        public override bool IsExecutionSource()
        {
            return true;
        }
    }
}