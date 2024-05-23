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
            Coroutine buff = playerController.HasBuff(input.id);
            if (buff != null)
            {
                StopCoroutine(buff);
                playerController.RemoveFromBuffs(input.id);
            }
            StartCoroutine(EffectOverTime(input, playerController.ApplyHeal, () => playerController.RemoveFromBuffs(input.id)));
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
        Coroutine buff = playerController.HasBuff(input.id);
        if (buff != null)
        {
            StopCoroutine(buff);
            playerController.RemoveFromBuffs(input.id);
            playerController.Stasis(false);
        }
        StartCoroutine(EffectOverTime(input, playerController.Stasis, () => playerController.RemoveFromBuffs(input.id)));
    }

    void SpeedUp(AbilityInput input)
    {
        Coroutine buff = playerController.HasBuff(input.id);
        if (buff != null)
        {
            StopCoroutine(buff);
            playerController.RemoveFromBuffs(input.id);
            playerController.ModifyMovementSpeed(input, false);
        }
        StartCoroutine(EffectOverTime(input, playerController.ModifyMovementSpeed, () => playerController.RemoveFromBuffs(input.id)));
    }

    void Cleanse(AbilityInput input = null)
    {
        playerController.RemoveAllDebuffs();
    }

    void Dash(AbilityInput input)
    {
        //use the mover/move from playercontroller after giving them parameters
        playerController.Dash(input);
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
            //MAYBE add a new variable called stackable and if the ability is stackable dont use this block, if its not stackable use the part etc etc
            Coroutine debuff = playerController.HasDebuff(input.id);
            if (debuff != null)
            {
                StopCoroutine(debuff);
                playerController.RemoveFromDebuffs(input.id);
            }
            
            playerController.AddToDebuffs(input.id, StartCoroutine(EffectOverTime(input, playerController.ApplyDamage, () => playerController.RemoveFromDebuffs(input.id))));
        }
    }

    void Stun(AbilityInput input)
    {
        Coroutine debuff = playerController.HasDebuff(input.id);
        if (debuff != null)
        {
            StopCoroutine(debuff);
            playerController.RemoveFromDebuffs(input.id);
            playerController.Stun(false);
        }
        playerController.AddToDebuffs(input.id, StartCoroutine(EffectOverTime(input, playerController.Stun, () => playerController.RemoveFromDebuffs(input.id))));
    }

    void Root(AbilityInput input)
    {
        Coroutine debuff = playerController.HasDebuff(input.id);
        if (debuff != null)
        {
            StopCoroutine(debuff);
            playerController.RemoveFromDebuffs(input.id);
            playerController.Root(false);
        }
        playerController.AddToDebuffs(input.id, StartCoroutine(EffectOverTime(input, playerController.Root, () => playerController.RemoveFromDebuffs(input.id))));
    }

    void Mark(AbilityInput input)
    {
        //StartCoroutine(MarkTracker(input.id, input.duration));
        Coroutine debuff = playerController.HasDebuff(input.id);
        if(debuff != null)
        {
            StopCoroutine(debuff);
            playerController.RemoveFromDebuffs(input.id);
            playerController.Mark(input, false);
        }
        playerController.AddToDebuffs(input.id, StartCoroutine(EffectOverTime(input, playerController.Mark, () => playerController.RemoveFromDebuffs(input.id))));
    }

    void SpawnUnit(AbilityInput input)//maybe make this into a targeting type so it can just use a prefab of the unit
    {
        //spawn the specific item using the id of the ability to pull from some sort of dictionary/db
    }

    void Slow(AbilityInput input)
    {
        Coroutine debuff = playerController.HasDebuff(input.id);
        if (debuff != null)
        {
            StopCoroutine(debuff);
            playerController.RemoveFromDebuffs(input.id);
            playerController.ModifyMovementSpeed(input, true);
        }
        input.magnitude = -input.magnitude;//TEST IF SLOWS WORK I DONT REMEMBER TESTING NOR DID THIS MAKE SENSE TO ME, cuz its already modified im modmovsped inside
        //re looking at things its neede as effectovertime always claims positive, so yea, turn the actions into no parameter types with lambda and put inputs in the lambda funcs
        //StartCoroutine(Slower(input.magnitude, input.duration));
        playerController.AddToDebuffs(input.id, StartCoroutine(EffectOverTime(input, playerController.ModifyMovementSpeed)));
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
    //callbacks are for removing the ability from the buff/debuff list when its done
    IEnumerator EffectOverTime(AbilityInput input, Action<AbilityInput, bool> effectFunc, Action callback = null)
    {
        effectFunc(input, true);
        yield return new WaitForSeconds(input.duration);
        effectFunc(input, false);
        if(callback != null)
        {
            callback();
        }
    }

    IEnumerator EffectOverTime(AbilityInput input, Action<AbilityInput> effectFunc, Action callback = null)
    {
        effectFunc(input);
        yield return new WaitForSeconds(input.duration);
        if (callback != null)
        {
            callback();
        }

    }

    IEnumerator EffectOverTime(AbilityInput input, Action<bool> effectFunc, Action callback = null)
    {
        effectFunc(true);
        yield return new WaitForSeconds(input.duration);
        effectFunc(false);
        if (callback != null)
        {
            callback();
        }
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
        public float magnitude, duration;
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
        public float magnitude = 0, duration = 0;
        
        public AbilityInput(string _id = "", float _magnitude = 0, float _duration = 0)
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
