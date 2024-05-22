using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CommandIndicator : MonoBehaviour
{
    [SerializeField]
    IndicatorInfo info;
    [SerializeField]
    Arrows arrows;
    [Range(0f, 1f)]
    public float progress;


    SpriteRenderer[] arrowRenderers;
    SpriteRenderer ringRenderer;

    Coroutine indicatorCoroutine;

    [SerializeField]
    bool debugMode;
    [SerializeField]
    Vector3 debugTarget;

    // Start is called before the first frame update
    void Start()
    {
        arrowRenderers = transform.GetChild(info.arrowChildIndex).GetComponentsInChildren<SpriteRenderer>();
        ringRenderer = transform.GetChild(info.ringChildIndex).GetComponent<SpriteRenderer>();

        transform.localScale = new Vector3 (info.size, info.size, 1f);

        //if(debugMode)
        //    indicatorCoroutine = StartCoroutine(IndicateTargetLocation(debugTarget));
    }

    // Update is called once per frame
    void Update()
    {
        //arrows.PositionArrows(progress);
    }


    public void IndicateLocation(Vector3 location)
    {
        if(indicatorCoroutine != null)
        {
            StopCoroutine(indicatorCoroutine);
            progress = 1;
            UpdateVisuals(progress);
            arrows.PositionArrows(progress);
        }
        StartCoroutine(IndicateTargetLocation(location));
    }

    IEnumerator IndicateTargetLocation(Vector3 target)
    {
        progress = 0;

        transform.position = target;

        while(progress < 1)
        {
            yield return new WaitForSeconds(info.animTickRate);
            progress += info.animSpeed;
            UpdateVisuals(progress);
            arrows.PositionArrows(progress);
        }
    }

    private void OnValidate()
    {
        transform.localScale = new Vector3(info.size, info.size, 1f);
        UpdateVisuals(progress);
        arrows.PositionArrows(progress);
    }

    public void UpdateVisuals(float progress)
    {
        Color color;
        if(arrowRenderers == null || arrowRenderers.Length <= 0)
        {
            arrowRenderers = transform.GetChild(info.arrowChildIndex).GetComponentsInChildren<SpriteRenderer>();
        }
        color = info.arrowColor;
        color.a = (1 - progress);
        foreach (SpriteRenderer renderer in arrowRenderers)
        {
            renderer.color = color;
        }
        if(ringRenderer == null)
        {
            ringRenderer = transform.GetChild(info.ringChildIndex).GetComponent<SpriteRenderer>();
        }
        color = info.ringColor;
        color.a = (1 - progress);
        ringRenderer.color = color;
    }


    [Serializable]
    public struct Arrows
    {
        public float basePos;
        
        public GameObject up, down, left, right;

        public void PositionArrows(float progress)//also add x rotation to rotate them into the ground maybe
        {
            Vector3 pos = Vector3.zero;
            pos.y = basePos * (1-progress/2);
            up.transform.localPosition = pos;
            down.transform.localPosition = -pos;
            pos.x = pos.y;
            pos.y = 0;
            right.transform.localPosition = pos;
            left.transform.localPosition = -pos;
        }
    }

    [Serializable]
    public struct IndicatorInfo
    {
        [SerializeField]
        public Color arrowColor, ringColor;
        [SerializeField]
        public float animSpeed, size, animTickRate;

        [SerializeField]
        public int arrowChildIndex, ringChildIndex;

    }
}
