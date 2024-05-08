using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollidingUnit
{
    public void Initialize(Vector3 target, Ability callerReference);

    public IEnumerator UnitBehaviour();//this is only necessary to make you remember to use it this way, there are no current calls to this via interface shenanigans
    /*
    public IEnumerator ProjectileBehaviour();

    public IEnumerator GroundTargetBehaviour();

    public IEnumerator UnitSpawnerBehaviour();
    */
}
