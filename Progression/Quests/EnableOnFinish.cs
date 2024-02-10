using UnityEngine;

namespace ProgressionSystem.Quests
{
    public class EnableOnFinish : RewardReceiver
    {
        public Behaviour[] BehavioursOnSuccess;
        public GameObject[] GameObjectsOnSuccess;
        public bool Enable;
    
        private void Awake()
        {
            foreach (var behaviour in BehavioursOnSuccess)
            {
                behaviour.enabled = !Enable;
            }

            foreach (var gameObj in GameObjectsOnSuccess)
            {
                gameObj.SetActive(!Enable);
            }
        }
    
        public override void ReceiveReward(Quest quest)
        {
            foreach (var behaviour in BehavioursOnSuccess)
            {
                behaviour.enabled = Enable;
            }

            foreach (var gameObj in GameObjectsOnSuccess)
            {
                gameObj.SetActive(Enable);
            }
        }
    }
}