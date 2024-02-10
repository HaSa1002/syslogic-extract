using UnityEngine;

namespace ProgressionSystem.Quests
{
    public class SetLightIntensity : RewardReceiver
    {
        public Light Light;
        public float Intensity;

        public void Awake()
        {
            Debug.Assert(Light);
            Light.intensity = 0;
        }

        public override void ReceiveReward(Quest quest)
        {
            Light.intensity = Intensity;
        }
    }
}