using System;
using System.Collections;
using UnityEngine;

public class GroundTargeterScript : MonoBehaviour, ICollidingUnit
{
    [SerializeField]
    float appearTime, activeTime, fadeTime;

    float tickRate;

    Ability abilityRef;
    public enum Shape { Circle, Box, CapsuleVertical, CapsuleHorizontal };

    [SerializeField]
    Shape shape;

    [SerializeField]
    Color baseColor, activeColor, fadeColor;

    Vector2 size;

    Coroutine behaviour;

    SpriteRenderer spriteRenderer;

    //Dictionary<Shape, object> overlapDictionary;
    Func<Collider2D> overlapFunc;

    public void Initialize(Vector3 target, Ability ability)
    {
        abilityRef = ability;

        transform.position = target;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = baseColor;

        #region Size Setup
        size = transform.localScale;
        if (GetComponent<Collider2D>())
        {
            if(shape == Shape.Circle)
            {
                size *= GetComponent<CircleCollider2D>().radius;
            }
            else if(shape == Shape.Box)
            {
                size *= GetComponent<BoxCollider2D>().size;
            }
            else if(shape == Shape.CapsuleVertical || shape == Shape.CapsuleHorizontal)
            { 
                size *= GetComponent<CapsuleCollider2D>().size;
            }
        }
        #endregion

        #region Overlap Function Setup
        LayerMask mask = LayerMask.GetMask("Player");
        //this section can be turned into a dict and the dict can just be used to access the method in unitbehaviour but that is not necessary for the current needs
        if (shape == Shape.Circle)
        {
            overlapFunc = () => Physics2D.OverlapCircle(transform.position, Mathf.Max(size.x, size.y), mask);
        }
        else if (shape == Shape.CapsuleVertical)
        {
            overlapFunc = () => Physics2D.OverlapCapsule(transform.position, size, CapsuleDirection2D.Vertical, transform.rotation.eulerAngles.z, mask);
        }
        else if (shape == Shape.CapsuleHorizontal)
        {
            overlapFunc = () => Physics2D.OverlapCapsule(transform.position, size, CapsuleDirection2D.Horizontal, transform.rotation.eulerAngles.z, mask);
        }
        else
        {
            overlapFunc = () => Physics2D.OverlapBox(transform.position, size, transform.rotation.eulerAngles.z, mask);
        }
        #endregion

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
            colorRef.a = .2f + ((timer/appearTime) * 0.8f);
            spriteRenderer.color = colorRef;
            //Debug.Log(timer);
        }
        //Do the thing
        //Debug.Log("Boom");

        //LayerMask mask = LayerMask.GetMask("Player");
        Collider2D hit = null;

        if(activeTime > 0)
        {
            spriteRenderer.color = activeColor;
            timer = activeTime;
            while (timer > 0)
            {
                hit = overlapFunc();
                if (hit)
                {
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
