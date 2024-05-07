using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollidingUnit
{
    public void Initialize(AbilityManager.Targeting targetingInformation, Vector3 target, Ability callerReference);

    public IEnumerator UnitBehaviour();
    /*
    public IEnumerator ProjectileBehaviour();

    public IEnumerator GroundTargetBehaviour();

    public IEnumerator UnitSpawnerBehaviour();
    */
}
