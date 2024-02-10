using System;
using Macrolayer;
using UnityEngine;

namespace Microlayer
{
    public class BakeryActor : Actor
    {

        private Bakery _bakery;
        private Cake _lastEjectedCake;
        private bool _clearEject;
        private bool _eject;

        protected override void Start()
        {
            base.Start();
            _bakery = Target.GetComponent<Bakery>();
            Debug.Assert(_bakery, "A target bakery must be assigned!", this);
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);

            _eject = (bool)Input[0];
            _bakery.RefillRate = (float)Input[1];
            _bakery.Heating = (bool)Input[2];

            if (_eject && _clearEject)
            {
                _bakery.EjectCake(out _lastEjectedCake);
                _clearEject = false;
            }

            if (!_lastEjectedCake) return;

            EmitProgressionValue(_eject);

        }

        public override void GarbageExecute()
        {
            base.GarbageExecute();
            _eject = false;
        }

        public void Update()
        {
            if (!_eject)
            {
                _clearEject = true;
            }
        }
    }
}
