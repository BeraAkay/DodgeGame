using System.Collections;
using UnityEngine;

public class ProjectileScript : MonoBehaviour, ICollidingUnit
{
    //AbilityManager.Targeting targetingInfo;

    Vector3 startingPosition, targetPosition;

    Ability abilityRef;

    int hitCount;
    [SerializeField]
    int maximumHitCount;
    [SerializeField]
    float unitSpeed;
    Coroutine behaviour;

    //LayerMask mask;
    GameObject source;
    
    public void Initialize(Vector3 target, Ability abilityReference, GameObject source)
    {
        this.source = source;
        //targetingInfo = callerReference.targetingInformation;
        targetPosition = target;
        abilityRef = abilityReference;

        transform.localPosition = Vector3.zero;
        startingPosition = transform.position;

        hitCount = 0;
        //mask = LayerMask.GetMask("Player");

        GetComponent<Collider2D>().includeLayers = abilityReference.targetingInformation.LayerMask;
        behaviour = StartCoroutine(UnitBehaviour());
    }

    public IEnumerator UnitBehaviour()
    {
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
        /*
        if((mask & 1 << collision.gameObject.layer) == 0)
        {
            //Debug.Log("non targetlayer hit");
            return;
        }
        */
        //Debug.Log("hit");

        if(collision.gameObject.TryGetComponent(out ICharacter character) && maximumHitCount >= hitCount)
        {
            //do hit stuff
            abilityRef.ApplyComponentEffects(character, source);
            hitCount++;
        }
        if (maximumHitCount <= hitCount)
        {
            //destroy self / back to pool
            StopCoroutine(behaviour);
            Destroy(gameObject);
        }
    }

}
