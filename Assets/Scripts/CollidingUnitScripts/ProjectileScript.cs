using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour, ICollidingUnit
{
    AbilityManager.Targeting targetingInfo;

    Vector3 startingPosition, targetPosition;

    Ability abilityRef;

    int hitCount;
    [SerializeField]
    int maximumHitCount;
    [SerializeField]
    float unitSpeed;
    Coroutine behaviour;


    public void Initialize(Vector3 target, Ability callerReference)
    {
        targetingInfo = callerReference.targetingInformation;
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
        float tStep = 1 / (distance / unitSpeed);
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
        if(maximumHitCount >= hitCount++)
        {
            //do hit stuff
            if (abilityRef)
                abilityRef.ApplyComponentEffects();
            else
                throw new Exception("Ability Reference Null in Projectile Script");

        }
        if (maximumHitCount >= hitCount)
        {
            //destroy self / back to pool
            StopCoroutine(behaviour);
            Destroy(gameObject);
        }
    }

}
