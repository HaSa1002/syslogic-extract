using Macrolayer;
using UnityEngine;

namespace Microlayer.Sensors
{
    public class BakeryTemperatureSensor : Sensor
    {
        private Bakery _bakery;
        
        protected override void Start()
        {
            base.Start();
            _bakery = MacroSensorPosition.GetComponent<Bakery>();
            Debug.Assert(_bakery, "A target bakery must be assigned!", this);
        }

        public override void Execute()
        {
            var output = new MicroData[] { _bakery.Temperature / Bakery.OverheatTemperature};
            PushData(output);
            UpdateEmissionValue(output);
        }
    }
}