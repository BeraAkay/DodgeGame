using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class AbilityManager : MonoBehaviour
{
    public static AbilityManager instance;

    public List<Ability> projectiles;

    public PlayerController playerController;

    public static Dictionary<AbilityComponent.Type, Action<AbilityInput>> componentDictionary;

    [SerializeField]
    static float tickRate = .5f;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

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
             (id) => playerController.RemoveFromBuffs(id)
             );
            BuffTarget(input, effect);
        }
    }

    void Blink(AbilityInput input)
    {
        playerController.Blink(input);
    }

    void Stasis(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Stasis(true),
            (id) => playerController.RemoveFromBuffs(id),
            () => playerController.Stasis(false)
            );
        BuffTarget(input, effect);
    }

    void SpeedUp(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.ModifyMovementSpeed(input, true),
            (id) => playerController.RemoveFromBuffs(id),
            () => playerController.ModifyMovementSpeed(input, false)
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
                (id) => playerController.RemoveFromDebuffs(id)
                );
            DebuffTarget(input, tickingEffect);
        }
    }

    void Stun(AbilityInput input)//untested
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Stun(true),
            (id) => playerController.RemoveFromDebuffs(id),
            () => playerController.Stun(false)
            );
        DebuffTarget(input, effect);
    }

    void Root(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Root(true),
            (id) => playerController.RemoveFromDebuffs(id),
            () => playerController.Root(false)
            );
        DebuffTarget(input, effect);
    }

    void Mark(AbilityInput input)//untested
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Mark(input, true),
            (id) => playerController.RemoveFromDebuffs(id),
            () => playerController.Mark(input, false)
            );
        DebuffTarget(input, effect);
    }

    void Slow(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.ModifyMovementSpeed(input, false),
            (id) => playerController.RemoveFromDebuffs(id),
            () => playerController.ModifyMovementSpeed(input, true)
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
        effect.UnlistCallback(input.id);
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
        effect.UnlistCallback(input.id);
    }

    void BuffTarget(AbilityInput input, Effect effect)
    {
        EffectInfo buffInfo = playerController.HasBuff(input.id);
        
        if(buffInfo != null && buffInfo.coroutine != null)
        {
            effect.UnlistCallback(input.id);
        }

        Coroutine buffCoroutine;
        if (effect.type == Effect.Type.Status)
        {
            buffCoroutine = StartCoroutine(EffectOverTime(input, effect));
        }
        else
        {
            buffCoroutine = StartCoroutine(TickingOverTime(input, effect));
        }

        buffInfo = new EffectInfo(buffCoroutine, effect);
        playerController.AddToBuffs(input.id, buffInfo);
    }

    void DebuffTarget(AbilityInput input, Effect effect)
    {
        EffectInfo debuffInfo = playerController.HasDebuff(input.id);

        if (debuffInfo != null && debuffInfo.coroutine != null)
        {
            effect.UnlistCallback(input.id);
        }

        Coroutine coroutine;
        if(effect.type == Effect.Type.Status)
        {
            coroutine = StartCoroutine(EffectOverTime(input, effect));
        }
        else
        {
            coroutine = StartCoroutine(TickingOverTime(input, effect));
        }

        debuffInfo = new EffectInfo(coroutine, effect);
        playerController.AddToDebuffs(input.id, debuffInfo);
    }

    //this is needed to avoid an apparently harmless error msg "Coroutine continue failure" when cleansing since coroutines need to be stopped in the script they are created.
    //This might be avoidable via changing the way yield new waitfor (x)
    //into a loop that tracks the time instead of a waitforsec with big X
    //that would reduce class dependency i guess??
    //but in this project it does not seem necessary, also stopping via ienumerator also works apparently
    public void CoroutineStopper(Coroutine coroutine)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
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

    public class Effect
    {
        public Action Apply, Undo;
        public Action<string> UnlistCallback;
        public enum Type { Status, Ticking };//idea, ticking effects should stack maybe?
        public Type type;

        public Effect(Type type, Action apply, Action<string> unlistCallback, Action undo = null)
        {
            this.type = type;
            Apply = apply;
            UnlistCallback = unlistCallback;
            Undo = undo;//this should not exist when ticking, maybe i can make 2 constructors if needed and if it has undo it is set to status, otherwise its ticking
        }
    }
    public class EffectInfo
    {
        //public string id;
        public Coroutine coroutine;
        public Effect effect;
        
        public EffectInfo(Coroutine crt, Effect eff)
        {
            coroutine = crt;
            effect = eff;
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
