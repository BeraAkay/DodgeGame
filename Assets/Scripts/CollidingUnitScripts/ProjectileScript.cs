using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    LayerMask mask;


    public void Initialize(Vector3 target, Ability callerReference)
    {
        targetingInfo = callerReference.targetingInformation;
        targetPosition = target;
        abilityRef = callerReference;

        transform.localPosition = Vector3.zero;
        startingPosition = transform.position;

        hitCount = 0;
        mask = LayerMask.GetMask("Player");
        behaviour = StartCoroutine(UnitBehaviour());
    }

    public IEnumerator UnitBehaviour()
    {
        /*
        float distance = Vector3.Distance(startingPosition, targetPosition);
        float t = 0;
        float tStep = 1 / (distance / unitSpeed);

        Renderer renderer = GetComponent<Renderer>();
        //lerp position to target
        while (t < 1 || renderer.isVisible == true)
        {
            t += tStep * 0.1f;
            transform.position = Vector3.LerpUnclamped(startingPosition, targetPosition, Mathf.Min(t, 4));//instead of stopping at a distance, just destroy self  when fully outside of screen
            yield return new WaitForFixedUpdate();
        }
        
        */

        
        GetComponent<Rigidbody2D>().velocity = (targetPosition - startingPosition).normalized * unitSpeed;
        //yield return null;
        yield return new WaitUntil(() => GetComponent<Renderer>().isVisible == true);
        //Debug.Log("appeared on screen");

        //maybe put some loop or smth here in case its multiple hits, but this project doesnt need it so its fine, only needed if the ability system will be used in another project
        yield return new WaitUntil(() => GetComponent<Renderer>().isVisible == false);
        //Debug.Log("went off screen");
        
        
        StopCoroutine(behaviour);
        Destroy(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if((mask & 1 << collision.gameObject.layer) == 0)
        {
            //Debug.Log("non player hit");
            return;
        }
        //Debug.Log("player hit");

        if(maximumHitCount >= hitCount++)
        {
            //do hit stuff
            abilityRef.ApplyComponentEffects();
        }
        if (maximumHitCount <= hitCount)
        {
            //destroy self / back to pool
            StopCoroutine(behaviour);
            Destroy(gameObject);
        }
    }

}
