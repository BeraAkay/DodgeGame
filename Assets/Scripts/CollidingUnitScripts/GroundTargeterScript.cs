using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTargeterScript : MonoBehaviour, ICollidingUnit
{
    [SerializeField]
    float unitLifetime;
    Ability abilityRef;


    public enum Shape { Circle, Box, CapsuleVertical, CapsuleHorizontal };

    [SerializeField]
    Shape shape;

    Vector2 size;

    Coroutine behaviour;

    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        Initialize(transform.position, null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Vector3 target, Ability ability)
    {
        abilityRef = ability;

        transform.position = target;

        spriteRenderer = GetComponent<SpriteRenderer>();
        
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

        behaviour = StartCoroutine(UnitBehaviour());
    }

    public IEnumerator UnitBehaviour()
    {
        float timer = 0;
        Color colorRef = spriteRenderer.color;
        colorRef.a = .2f;
        while (timer < unitLifetime)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            colorRef.a = .2f + ((timer/unitLifetime) * 0.8f);
            spriteRenderer.color = colorRef;
            //Debug.Log(timer);
        }
        //Do the thing
        //Debug.Log("Boom");

        LayerMask mask = LayerMask.GetMask("Player");
        Collider2D hit;
        if(shape == Shape.Circle)
        {
            hit = Physics2D.OverlapCircle(transform.position, Mathf.Max(size.x, size.y), mask);
        }
        else if (shape == Shape.CapsuleVertical)
        {
            Debug.Log(size);
            hit = Physics2D.OverlapCapsule(transform.position, size, CapsuleDirection2D.Vertical, transform.rotation.eulerAngles.z, mask);
        }
        else if (shape == Shape.CapsuleHorizontal)
        {
            Debug.Log(size);
            hit = Physics2D.OverlapCapsule(transform.position, size, CapsuleDirection2D.Horizontal, transform.rotation.eulerAngles.z, mask);
        }
        else
        {
            hit = Physics2D.OverlapBox(transform.position, size, transform.rotation.eulerAngles.z, mask);
        }
        
        if (hit)
        {
            Debug.Log("Hit the player");
            if (abilityRef)
                abilityRef.ApplyComponentEffects();
            else
                throw new Exception("Ability Reference Null in GroundTargeter Script");
        }
        else
        {
            Debug.Log("missed");
        }

    }
}
