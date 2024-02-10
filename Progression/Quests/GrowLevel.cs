using UnityEngine;

namespace ProgressionSystem.Quests
{
    public class GrowLevel : RewardReceiver
    {

        public bool SetVisibility = true;
        
        public void Start()
        {
            Debug.Assert(GetComponent<Collider>());
            if (SetVisibility)
            {
                gameObject.SetActive(false);
            }
            ReceiveReward(null);
        }

        public override void ReceiveReward(Quest quest)
        {
            if (SetVisibility)
            {
                gameObject.SetActive(true);
            }
            transform.root.GetComponentInChildren<CameraLookTargetMacro>(true).GrowBounds(GetComponent<Collider>().bounds);
        }
    }
}