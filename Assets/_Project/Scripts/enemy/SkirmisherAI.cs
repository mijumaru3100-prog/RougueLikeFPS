using UnityEngine;

public class SkirmisherAI : EnemyAI
{
    protected override void UpdateMovement(float dist, bool canSee)
    {
        agent.updateRotation = false; 
        
        if (canSee)
        {
            if (dist < keepDistance - 1f)
            {
                Flee();
            }
            else
            {
                if (agent.hasPath) 
                {
                    agent.ResetPath(); 
                }
                agent.destination = transform.position; 
                agent.velocity = Vector3.zero;          
            }
        }
        else
        {
            agent.destination = target.position;
        }
    }

    protected override float GetAnimationSpeed(float currentDist)
    {
        float buffer = 1.0f; 

        if (currentDist < keepDistance - buffer)
        {
            return -0.5f;
        }
        else if (currentDist > keepDistance + buffer)
        {
            return 0.5f;
        }
        else 
        {
            return anim.GetFloat(SpeedHash);
        }
    }

    protected override bool IsFleeing()
    {
        return Vector3.Distance(transform.position, target.position) < keepDistance;
    }
}
