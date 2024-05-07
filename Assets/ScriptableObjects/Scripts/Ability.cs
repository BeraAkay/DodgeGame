using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu]
public class Ability : ScriptableObject
{
    public new string name;

    public int cooldown;

    public AbilityManager.Targeting targetingInformation;

    public AbilityManager.AbilityComponent[] activeComponents;

    public AbilityManager.AbilityComponent[] passiveComponents;
    public void Use(Transform userTransform, Vector3 target = default(Vector3))
    {
        //instantiate collidingUnit and give it the activeComponents
        Instantiate(targetingInformation.collidingUnit, userTransform).GetComponent<ICollidingUnit>().Initialize(targetingInformation, target, this);

    }
    public void Use()
    {

    }

    public void ApplyComponentEffects()
    {

    }

    public void ActivatePassives(bool flag)
    {
        
    }

}
