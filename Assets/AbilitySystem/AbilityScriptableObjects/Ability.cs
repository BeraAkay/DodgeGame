using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu]
public class Ability : ScriptableObject
{
    public new string name;

    public int cooldown;

    public AbilityManager.Targeting targetingInformation;

    public AbilityManager.AbilityComponent[] activeComponents;

    public AbilityManager.AbilityComponent[] passiveComponents;
    public void Use(GameObject source, Vector3 targetPosition = default)
    {
        //instantiate collidingUnit and give it the activeComponents
        Instantiate(targetingInformation.collidingUnit, source.transform).GetComponent<ICollidingUnit>().Initialize(targetPosition, this, source);

    }
    public void Use(ICharacter target, GameObject source)
    {
        ApplyComponentEffects(target, source);
    }

    public void ApplyComponentEffects(ICharacter target, GameObject source)
    {
        foreach(var component in activeComponents)
        {
            component.Apply(name + "_" + component.CompType.ToString(), target, source);
        }
    }


    public void ActivatePassives(bool flag)
    {
        
    }

}
