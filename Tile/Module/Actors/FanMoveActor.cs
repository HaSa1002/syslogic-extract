using UnityEngine;

namespace Microlayer
{
    public class FanMoveActor : Actor
    {
        private float _bakery;
        private float _flat;
        private MicroData _position;

        protected override void Start()
        {
            base.Start();

            _flat = Target.position.x;
            _bakery = Target.GetChild(Target.childCount - 1).position.x;
        }

        public override void Execute()
        {
            UpdateEmissionValue(Input);
            _position = Input[0];
        }

        private void Update()
        {
            if (_position >= 0f)
            {
                var lerp = Mathf.Lerp(_flat, _bakery, (float)_position);
                Target.position = new Vector3(lerp, Target.position.y, Target.position.z);
            }
            
        }

        public MicroData GetInput()
        {
            if (_position >= 0f) return _position;
            return MicroData.Low[0];
        }
    }
}