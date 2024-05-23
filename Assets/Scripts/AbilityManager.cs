using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class AbilityManager : MonoBehaviour
{
    public List<Ability> projectiles;

    public PlayerController playerController;

    public static Dictionary<AbilityComponent.Type, Action<AbilityInput>> componentDictionary;

    [SerializeField]
    static float tickRate = .5f;

    void Awake()
    {
        playerController = FindAnyObjectByType<PlayerController>();

        componentDictionary = new Dictionary<AbilityComponent.Type, Action<AbilityInput>>();
        //enemy
        componentDictionary[AbilityComponent.Type.Damage] = Damage;
        componentDictionary[AbilityComponent.Type.Stun] = Stun;
        componentDictionary[AbilityComponent.Type.Root] = Root;
        componentDictionary[AbilityComponent.Type.Mark] = Mark;
        componentDictionary[AbilityComponent.Type.Slow] = Slow;

        //player
        componentDictionary[AbilityComponent.Type.Heal] = Heal;
        componentDictionary[AbilityComponent.Type.SpeedUp] = SpeedUp;
        componentDictionary[AbilityComponent.Type.Blink] = Blink;
        componentDictionary[AbilityComponent.Type.Stasis] = Stasis;
        componentDictionary[AbilityComponent.Type.Cleanse] = Cleanse;
        componentDictionary[AbilityComponent.Type.Dash] = Dash;

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
            Effect effect = new Effect(
             Effect.Type.Ticking,
             () => playerController.ApplyHeal(input),
             () => playerController.RemoveFromBuffs(input.id)
             );
            BuffTarget(input, effect);
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
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Stasis(true),
            () => playerController.Stasis(false),
            () => playerController.RemoveFromBuffs(input.id)
            );
        BuffTarget(input, effect);
    }

    void SpeedUp(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.ModifyMovementSpeed(input, true),
            () => playerController.ModifyMovementSpeed(input, false),
            () => playerController.RemoveFromBuffs(input.id)
            );
        BuffTarget(input, effect);
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
        else//damage over time, untested
        {
            //MAYBE add a new variable called stackable and if the ability is stackable dont use this block, if its not stackable use the part etc etc
            Effect tickingEffect = new Effect(
                Effect.Type.Ticking,
                () => playerController.ApplyDamage(input),
                () => playerController.RemoveFromDebuffs(input.id)
                );
            DebuffTarget(input, tickingEffect);
        }
    }

    void Stun(AbilityInput input)//untested
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Stun(true),
            () => playerController.Stun(false),
            () => playerController.RemoveFromDebuffs(input.id)
            );
        DebuffTarget(input, effect);
    }

    void Root(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Root(true),
            () => playerController.Root(false),
            () => playerController.RemoveFromDebuffs(input.id)
            );
        DebuffTarget(input, effect);
    }

    void Mark(AbilityInput input)//untested
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Mark(input, true),
            () => playerController.Mark(input, false),
            () => playerController.RemoveFromDebuffs(input.id)
            );
        DebuffTarget(input, effect);
    }

    void Slow(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.ModifyMovementSpeed(input, false),
            () => playerController.ModifyMovementSpeed(input, true),
            () => playerController.RemoveFromDebuffs(input.id)
            );
        DebuffTarget(input, effect);
    }
    #endregion

    #region Effect Over Time Functions
    //callbacks are for removing the ability from the buff/debuff list when its done
    IEnumerator EffectOverTime(AbilityInput input, Effect effect)
    {
        effect.Apply();
        yield return new WaitForSeconds(input.duration);
        effect.Undo();
        effect.UnlistCallback();
    }

    IEnumerator TickingOverTime(AbilityInput input, Effect effect)
    {
        float remainingDuration = input.duration;
        while (remainingDuration > 0)
        {
            effect.Apply();
            yield return new WaitForSeconds(tickRate);
            remainingDuration -= tickRate;
        }
        effect.UnlistCallback();
    }

    void BuffTarget(AbilityInput input, Effect effect)
    {
        Coroutine buff = playerController.HasBuff(input.id);
        if (buff != null)
        {
            StopCoroutine(buff);
            effect.UnlistCallback();
            if (effect.Undo != null)
                effect.Undo();//find a way to remove this and just edit buff time
        }
        if (effect.type == Effect.Type.Status)
            buff = StartCoroutine(EffectOverTime(input, effect));
        else
            buff = StartCoroutine(TickingOverTime(input, effect));
        playerController.AddToBuffs(input.id, buff);
    }

    void DebuffTarget(AbilityInput input, Effect effect)
    {
        Coroutine debuff = playerController.HasDebuff(input.id);
        if (debuff != null)
        {
            StopCoroutine(debuff);
            effect.UnlistCallback();
            if(effect.Undo != null)
                effect.Undo();//find a way to remove this and just edit debuff time
        }
        if(effect.type == Effect.Type.Status)
            debuff = StartCoroutine(EffectOverTime(input, effect));
        else
            debuff = StartCoroutine(TickingOverTime(input, effect));
        playerController.AddToDebuffs(input.id, debuff);
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
        [SerializeField]
        float magnitude, duration;

        public float Magnitude
        {
            get { return magnitude; }
        }
        public float Duration
        {
            get { return duration;}
        }
        public enum Type { Damage, Stun, Root, Mark, Slow, Heal, SpeedUp, Blink, Stasis, Cleanse, Dash };
        [SerializeField]
        Type type;
        public Type CompType
        {
            get { return type; }
        }

        [HideInInspector]
        public readonly List<Type> invokeEachTick = new List<Type> { Type.Damage, Type.Heal };

        [HideInInspector]
        public readonly List<Type> statuses = new List<Type> { Type.Stun, Type.Root, Type.Mark, Type.Slow, Type.SpeedUp, Type.Stasis};
        public void Apply(string id)
        {
            //Debug.Log("Applying Ability Component: " + type.ToString() +" Magnitude: " + magnitude.ToString() + " Duration: " + duration.ToString());
            componentDictionary[type].Invoke(new AbilityInput(id, magnitude, duration));
        }

        public void ForceInvoke(string id)
        {
            componentDictionary[type].Invoke(new AbilityInput(id, magnitude, duration));
        }
    }

    class Effect
    {
        public Action Apply, UnlistCallback, Undo;
        public enum Type { Status, Ticking };//idea, ticking effects should stack maybe?
        public Type type;

        public Effect(Type type, Action apply, Action unlistCallback, Action undo = null)
        {
            this.type = type;
            Apply = apply;
            UnlistCallback = unlistCallback;
            Undo = undo;//this should not exist when ticking, maybe i can make 2 constructors if needed and if it has undo it is set to status, otherwise its ticking
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
