using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows;

[DefaultExecutionOrder(0)]
public class AbilityManager : MonoBehaviour
{
    public List<Ability> projectiles;

    public PlayerController playerController;

    public static Dictionary<AbilityComponent.Type, Action<AbilityInput>> componentDictionary;


    void Awake()
    {
        playerController = FindAnyObjectByType<PlayerController>();

        componentDictionary = new Dictionary<AbilityComponent.Type, Action<AbilityInput>>();
        //enemy
        componentDictionary[AbilityComponent.Type.Damage] = Damage;
        componentDictionary[AbilityComponent.Type.Stun] = Stun;
        componentDictionary[AbilityComponent.Type.Root] = Root;
        componentDictionary[AbilityComponent.Type.Mark] = Mark;
        componentDictionary[AbilityComponent.Type.Spawn] = SpawnUnit;
        componentDictionary[AbilityComponent.Type.Slow] = Slow;

        //player
        componentDictionary[AbilityComponent.Type.Heal] = Heal;
        componentDictionary[AbilityComponent.Type.SpeedUp] = SpeedUp;
        componentDictionary[AbilityComponent.Type.Blink] = Blink;
        componentDictionary[AbilityComponent.Type.Stasis] = Stasis;
        componentDictionary[AbilityComponent.Type.Cleanse] = Cleanse;
        componentDictionary[AbilityComponent.Type.Dash] = Dash;

    }

    void Update()
    {

    }

    #region Player Abilities
    void Heal(AbilityInput input)
    {
        if (input.duration == 0)//instant heal
        {
            playerController.ApplyHeal(input.magnitude);
        }
        else//heal over time
        {
            StartCoroutine(EffectOverTime(input, playerController.ApplyHeal));
        }
    }

    void Blink(AbilityInput input)
    {
        Vector3 newPosition = Vector3.zero;
        newPosition.z = playerController.transform.position.z;
        //calculate new position using the mouseposition and distance and prob just lerp the vector.


        playerController.transform.position = newPosition;
    }

    void Stasis(AbilityInput input)
    {

    }

    void SpeedUp(AbilityInput input)
    {
        StartCoroutine(EffectOverTime(input, playerController.ModifyMovementSpeed));
    }

    void Cleanse(AbilityInput input = null)
    {
        playerController.RemoveAllDebuffs();
    }

    void Dash(AbilityInput input)
    {
        //use the mover/move from playercontroller after giving them parameters
    }
    #endregion

    #region Enemy Abilities
    void Damage(AbilityInput input)
    {
        if(input.duration == 0)//instant damage 
        {
            playerController.ApplyDamage(input.magnitude);
        }
        else//damage over time
        {
            StartCoroutine(EffectOverTime(input, playerController.ApplyDamage));
        }
    }

    void Stun(AbilityInput input)
    {
        StartCoroutine(EffectOverTime(input, playerController.Stun));
    }

    void Root(AbilityInput input)
    {
        StartCoroutine(EffectOverTime(input, playerController.Root));
    }

    void Mark(AbilityInput input)
    {
        //StartCoroutine(MarkTracker(input.id, input.duration));
        StartCoroutine(EffectOverTime(input, playerController.Mark));
    }

    void SpawnUnit(AbilityInput input)//maybe make this into a targeting type so it can just use a prefab of the unit
    {
        //spawn the specific item using the id of the ability to pull from some sort of dictionary/db
    }

    void Slow(AbilityInput input)
    {
        input.magnitude = -input.magnitude;
        //StartCoroutine(Slower(input.magnitude, input.duration));
        StartCoroutine(EffectOverTime(input, playerController.ModifyMovementSpeed));
    }
    #endregion

    #region Effect Over Time Coroutines
    /*
    IEnumerator Slower(int magnitude, int duration)
    {
        playerController.ModifyMovementSpeed(-magnitude);
        yield return new WaitForSeconds(duration);
        playerController.ModifyMovementSpeed(magnitude);
    }

    IEnumerator MarkTracker(string id, int duration)
    { 
        playerController.AddMark(id);
        yield return new WaitForSeconds(duration);
        playerController.RemoveMark(id);
    }
    */

    IEnumerator EffectOverTime(AbilityInput input, Action<AbilityInput, bool> effectFunc)
    {
        effectFunc(input, true);
        yield return new WaitForSeconds(input.duration);
        effectFunc(input, false);
    }

    IEnumerator EffectOverTime(AbilityInput input, Action<AbilityInput> effectFunc)
    {
        effectFunc(input);
        yield return new WaitForSeconds(input.duration);

    }

    IEnumerator EffectOverTime(AbilityInput input, Action<bool> effectFunc)
    {
        effectFunc(true);
        yield return new WaitForSeconds(input.duration);
        effectFunc(false);
    }

    #endregion

    #region Ability Systems Classes
    //Feel like the effect func of effect over time ienumerators can be embedded into one of these but i might look into that later
    //same goes for targeting but thats not needed in this project, an interface can be made and the inputs/abilities can just interact with interface methods and certain classes
    //would have the interface (in this case only PlayerController would use it so its not needed, but for future reference: if you want to take this script and make it more universal
    //across projects that would be a good thing to add)
    [Serializable]
    public class AbilityComponent
    {
        //public string name;
        public int magnitude, duration;
        public enum Type { Damage, Stun, Root, Mark, Spawn, Slow, Heal, SpeedUp, Blink, Stasis, Cleanse, Dash };
        public Type type;

        public void InvokeComponent(string id)
        {
            componentDictionary[type].Invoke(new AbilityInput(id, magnitude, duration));
        }
    }

    class ComponentActionPair
    {
        public AbilityComponent.Type componentType;
        public Action function;
    }

    public class AbilityInput
    {
        public string id = "";
        public int magnitude = 0, duration = 0;
        
        public AbilityInput(string _id = "", int _magnitude = 0, int _duration = 0)
        {
            id = _id;
            magnitude = _magnitude;
            duration = _duration;
        }
    }
    [Serializable]//, RequireComponent(typeof(Collider2D))] reqComp doesnt work here apparently, sadly
    public class Targeting
    {
        public enum Type { TargetGround, Projectile , UnitSpawn, Laser, Self };//this is unused
        public Type type;//this is unused 

        public GameObject collidingUnit;//unit that has a collider such as a projectile

    }

    #endregion

}
