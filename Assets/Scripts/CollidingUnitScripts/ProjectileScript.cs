using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour, ICollidingUnit
{
    AbilityManager.Targeting targetingInfo;

    Vector3 startingPosition, targetPosition;

    Ability abilityRef;

    int hitCount;
    Coroutine behaviour;


    public void Initialize(AbilityManager.Targeting targetingInformation, Vector3 target, Ability callerReference)
    {
        targetingInfo = targetingInformation;
        targetPosition = target;
        abilityRef = callerReference;

        transform.localPosition = Vector3.zero;
        startingPosition = transform.position;

        hitCount = 0;
        behaviour = StartCoroutine(UnitBehaviour());
    }

    public IEnumerator UnitBehaviour()
    {
        float distance = Vector3.Distance(startingPosition, targetPosition);
        float t = 0;
        float tStep = 1 / (distance / targetingInfo.unitSpeed);
        //lerp position to target
        while (t < 2)
        {
            t += tStep * 0.1f;
            transform.position = Vector3.LerpUnclamped(startingPosition, targetPosition, Mathf.Min(t, 4));//instead of stopping at a distance, just destroy self  when fully outside of screen
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(targetingInfo.maximumHitCount >= hitCount++)
        {
            //do hit stuff
            abilityRef.ApplyComponentEffects();

        }
        if (targetingInfo.maximumHitCount >= hitCount)
        {
            //destroy self / back to pool
            StopCoroutine(behaviour);
            Destroy(gameObject);
        }
    }

}
