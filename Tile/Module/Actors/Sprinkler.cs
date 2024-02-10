using System.Collections.Generic;
using UnityEngine;

namespace Microlayer
{
    public class Sprinkler : Actor
    {

        //public ParticleSystem LeftOverSmoke;

        private const float ExtinguishTime = 10;

        private List<ParticleSystem> _fires = new();
        private ParticleSystem _water;
        private List<float> _fireRates = new();
        private float _waterRate;
        private float _waterValve;
        //private float _leftOverSmokeRate;

        private float _fireExtinguishTime;


        protected override void Start()
        {
            base.Start();
            _water = Target.GetChild(0).GetComponent<ParticleSystem>();
            var emission = _water.emission;
            _waterRate = emission.rateOverTime.constant;
            emission.rateOverTime = 0;

            _fires.Add(Target.GetComponent<ParticleSystem>());
            _fireRates.Add(_fires[0].emission.rateOverTime.constant);

            for (int i = 1; i < Target.childCount; i++)
            {
                var systems = Target.GetChild(i).GetComponentsInChildren<ParticleSystem>(true);
                _fires.AddRange(systems);
                foreach (var system in systems)
                {
                    _fireRates.Add(system.emission.rateOverTime.constant);
                    system.Play(false);
                }
            }

            /*if (!LeftOverSmoke)
        {
            Debug.LogWarning("No leftOverSmoke assigned.");
            Debug.DebugBreak();
            return;
        }

        var smokeEmission = LeftOverSmoke.emission;
        //_leftOverSmokeRate = smokeEmission.rateOverTime.constant;
        //smokeEmission.rateOverTime = 0;
        LeftOverSmoke.Play(true);*/
        }

        public override void OnGraphRebuildRequested()
        {
            base.OnGraphRebuildRequested();
            if (Input.Count == 0)
            {
                Input.Add(0);
            }
            else
            {
                Input[0] = 0;
            }
            Execute();
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            _waterValve = (float)Input[0];
            var emission = _water.emission;
            emission.rateOverTime = (float)Input[0] * _waterRate;
        }

        private void Update()
        {
            if (!_fires[0].emission.enabled) return;

            var duration = _fires[0].main.startLifetime.constant;

            var fireReduction = _waterValve > 0.1f ? Time.deltaTime * _waterValve : Time.deltaTime * (_waterValve - 0.1f);
            _fireExtinguishTime = Mathf.Clamp(_fireExtinguishTime + fireReduction, 0, ExtinguishTime + duration);
            var fireRate = Mathf.Min(_fireExtinguishTime / ExtinguishTime, 1);

            // Add particle effect duration to ensure progression waits for the fire to be fully extinguished
            var progression = _fireExtinguishTime / (ExtinguishTime + duration);

            for (int i = 0; i < _fires.Count; i++)
            {
                var emission = _fires[i].emission;
                emission.rateOverTime = _fireRates[i] * (1 - fireRate);
                emission.enabled = progression < 1;
            }

            /*if (LeftOverSmoke)
        {
            var smokeEmission = LeftOverSmoke.emission;
            //smokeEmission.rateOverTime = _leftOverSmokeRate;
        }*/

            EmitProgressionValue(progression);
        }
    }
}
