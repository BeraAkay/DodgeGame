using System;
using System.Collections;
using UnityEngine;

public class LaserScript : MonoBehaviour, ICollidingUnit //this seems useless, as it can be mimicked by groundtargeter
{
    [SerializeField]
    float appearTime, activeTime, fadeTime;

    [SerializeField]
    Color baseColor, activeColor, fadeColor;

    float tickRate = 0.5f;

    Ability abilityRef;
    Vector2 size;
    Coroutine behaviour;
    SpriteRenderer spriteRenderer;
    Func<Collider2D> overlapFunc;

    public void Initialize(Vector3 target, Ability ability)
    {
        abilityRef = ability;

        //transform.LookAt(target, Vector3.forward);

        Vector3 dir = (target - transform.position).normalized;
        float angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.forward);
        //transform.RotateAround(Vector3.forward, angle);
        //Debug.Log(angle);
        transform.transform.rotation = Quaternion.identity;
        transform.Rotate(new Vector3(0, 0, angle));


        size = transform.localScale;
        transform.position += dir * size.y / 2;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = baseColor;


        LayerMask mask = LayerMask.GetMask("Player");
        overlapFunc = () => Physics2D.OverlapBox(transform.position, size, transform.rotation.eulerAngles.z, mask);

        behaviour = StartCoroutine(UnitBehaviour());
    }

    public IEnumerator UnitBehaviour()
    {
        float timer = 0;
        Color colorRef = spriteRenderer.color;
        colorRef.a = .2f;
        while (timer < appearTime)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            colorRef.a = .2f + ((timer / appearTime) * 0.8f);
            spriteRenderer.color = colorRef;
            //Debug.Log(timer);
        }
        //Do the thing
        //Debug.Log("Boom");

        //LayerMask mask = LayerMask.GetMask("Player");
        Collider2D hit = null;

        if (activeTime > 0)
        {
            spriteRenderer.color = activeColor;
            timer = activeTime;
            while (timer > 0)
            {
                hit = overlapFunc();
                if (hit)
                {
                    Debug.Log("hit player");
                    abilityRef.ApplyComponentEffects();
                }
                yield return new WaitForSeconds(tickRate);
                timer -= tickRate;
            }
        }
        else
        {
            hit = overlapFunc();//test it
            if (hit)
            {
                Debug.Log("hit player");
                abilityRef.ApplyComponentEffects();
            }
        }


        colorRef = fadeColor;
        timer = fadeTime;
        while (timer > 0)
        {
            yield return new WaitForFixedUpdate();
            timer -= Time.fixedDeltaTime;
            colorRef.a = (timer / fadeTime);
            spriteRenderer.color = colorRef;
        }

        StopCoroutine(behaviour);
        Destroy(gameObject);
    }
}
