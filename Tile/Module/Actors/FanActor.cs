using UnityEngine;

namespace Microlayer
{
    public class FanActor : Actor
    {
        public ParticleSystem Smoke;

        private const float Speed = 1440;
        private Transform _blades;
        private Transform _fixture;

        private float _speed;
        private float _tiltTime;

        protected override void Start()
        {
            base.Start();
            _fixture = Target.GetChild(0).GetChild(0);
            _blades = _fixture.GetChild(0);
            Debug.Assert(_blades, "A change was made to the fan but the code expects a different structure. Please update it.");

            if (Smoke) return;

            Debug.LogWarning("No smoke/move actor was assigned.", this);
        }

        public override void Execute()
        {
            _speed = (float)Input[0];
            EmitProgressionValue(!Smoke.IsAlive(true));
            UpdateEmissionValue(Input);
        }

        public override void OnGraphRebuildRequested()
        {
            base.OnGraphRebuildRequested();
            _speed = 0;
        }

        private void Update()
        {
            _blades.Rotate(Vector3.forward,Speed * _speed * Time.deltaTime);
            _tiltTime += Time.deltaTime * _speed;
            _fixture.rotation = Quaternion.AngleAxis(Mathf.Sin(_tiltTime) * 45, Vector3.up);
            if (_speed > 0.99f && Smoke)
            {
                Smoke.Stop(true);
            }
        }
    }
}
