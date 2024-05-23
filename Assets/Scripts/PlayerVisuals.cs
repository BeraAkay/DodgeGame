using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    Color baseColor;

    [SerializeField]
    Animator animator;

    [SerializeField]
    float colorFlashSpeed;

    Coroutine colorCoroutine;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
    }

    void Update()
    {
        
    }

    public void ScreenShake()
    {

    }

    public void PlayerShake()
    {

    }

    public void HitReaction()
    {
        animator.SetTrigger("Hit");
    }

    public void FlashColor()
    {
        FlashColor(Color.red);
    }

    public void FlashColor(Color color)
    {
        if(colorCoroutine != null)
        {
            StopCoroutine(colorCoroutine);
        }
        colorCoroutine = StartCoroutine(FlashColorCoroutine(color));
    }

    IEnumerator FlashColorCoroutine(Color color)
    {
        spriteRenderer.color = color;
        float t = 0;
        while (spriteRenderer.color != baseColor)
        {
            spriteRenderer.color = Color.Lerp(color, baseColor, t);
            yield return new WaitForFixedUpdate();
            t += colorFlashSpeed; 
        }
    }
}
