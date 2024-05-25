using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public Ability ability;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Shooter());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Shooter()
    {
        for(; ; )
        {
            ability.Use(gameObject, PlayerController.Instance.Position);
            yield return new WaitForSeconds(ability.cooldown);
        }
    }
}
