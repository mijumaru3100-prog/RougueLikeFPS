using UnityEngine;

public class ChargerAI : EnemyAI
{
    protected override void UpdateMovement(float dist, bool canSee)
    {
        agent.updateRotation = true;
        agent.destination = target.position;
    }

    protected override float GetAnimationSpeed(float currentDist)
    {
        return 0.5f;
    }
}
