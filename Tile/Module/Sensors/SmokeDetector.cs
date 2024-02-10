using UnityEngine;

namespace Microlayer
{
    public class SmokeDetector : Sensor
    {
        private ParticleSystem _smoke;

        protected override void Start()
        {
            base.Start();
            _smoke = MacroSensorPosition.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
        }

        public override void Execute()
        {
            var value = new MicroData[] { _smoke.isPlaying };
            PushData(value);
            UpdateEmissionValue(value);
        }
    }
}
