using System;
using UnityEngine;
using Random = System.Random;

[CreateAssetMenu(menuName = "MazeLogic/AlternatingMazeLogic")]
public class AlternatingMazeLogic : MazeLogic {

    private static Random rand = new Random();
    /// <summary>
    /// Returns the next non-repeating target for the subject.
    /// </summary>
    /// <param name="currentTarget">Index of the current target</param>
    /// <returns>Index of the next target</returns>
    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        // minimally inclusive, maximally exclusive, therefore you will get random numbers between 0 to the number of rewards
        int nextTarget = rand.Next(rewards.Length);

        // retries if the random target number generated is the same as the current target number
        float currentZ = 0f;
        if (currentTarget > -1)
        {
            RewardArea currentReward = rewards[currentTarget];
            currentZ = currentReward.transform.position.z;
            Debug.Log($"Current target posstion: {currentReward.transform.position}");
        }
        RewardArea nextReward = null;
        while (rewards.Length != 1) {
            nextTarget = rand.Next(0, rewards.Length);
            if (nextTarget == currentTarget)
            {
                continue;
            }
            else
            {
                nextReward = rewards[nextTarget];
                if (Math.Abs(nextReward.transform.position.z - currentZ) > Math.Abs(currentZ))
                {
                    break;
                }
            }
            
        }
        Debug.Log($"currentTarget: {currentTarget}");
        Debug.Log($"nextTarget: {nextTarget}");
        
        if (nextReward != null) 
        {
            Debug.Log($"next target: {nextReward.transform.position}");
        }
        return nextTarget;
    }

    public override Sprite GetTargetImage(RewardArea[] rewards, int targetIndex) {
        return rewards[targetIndex].cueImage;
    }

    public override bool IsTrialCompleteAfterCurrentTask(bool currentTaskSuccess) {
        return currentTaskSuccess; // 1 poster per trial
    }

    public override void Setup(RewardArea[] rewards) {
        //nothing to do here
    }

    public override bool ShowCue(int targetIndex) {
        return true;
    }
}
