using UnityEngine;

namespace ProgressionSystem
{
    public abstract class RewardReceiver : MonoBehaviour
    {
        public abstract void ReceiveReward(Quest quest);
    }
}