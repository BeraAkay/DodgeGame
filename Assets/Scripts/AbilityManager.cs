using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(0)]
public class AbilityManager : MonoBehaviour
{
    public static AbilityManager instance;

    public List<Ability> projectiles;

    public static Dictionary<AbilityComponentData.Type, Action<AbilityInput>> componentDictionary;

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

        componentDictionary = new Dictionary<AbilityComponentData.Type, Action<AbilityInput>>();
        //negative
        componentDictionary[AbilityComponentData.Type.Damage] = Damage;
        componentDictionary[AbilityComponentData.Type.Stun] = Stun;
        componentDictionary[AbilityComponentData.Type.Root] = Root;
        componentDictionary[AbilityComponentData.Type.Mark] = Mark;
        componentDictionary[AbilityComponentData.Type.SlowFlat] = SlowFlat;
        componentDictionary[AbilityComponentData.Type.SlowMult] = SlowMult;

        //positive
        componentDictionary[AbilityComponentData.Type.Heal] = Heal;
        componentDictionary[AbilityComponentData.Type.SpeedUpFlat] = SpeedUpFlat;
        componentDictionary[AbilityComponentData.Type.SpeedUpMult] = SpeedUpMult;
        componentDictionary[AbilityComponentData.Type.Blink] = Blink;
        componentDictionary[AbilityComponentData.Type.Stasis] = Stasis;
        componentDictionary[AbilityComponentData.Type.Cleanse] = Cleanse;
        componentDictionary[AbilityComponentData.Type.Dash] = Dash;

    }

    #region Player Abilities
    void Heal(AbilityInput input)
    {
        if (input.duration == 0)//instant heal
        {
            input.target.ApplyHeal(input.magnitude);
        }
        else//heal over time
        {
            Effect effect = new Effect(
             Effect.Type.Ticking,
             () => input.target.ApplyHeal(input.magnitude),
             (id) => input.target.RemoveFromBuffs(id)
             );
            BuffTarget(input, effect);
        }
    }

    void Blink(AbilityInput input)
    {
        input.target.Blink(input);
    }

    void Stasis(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => input.target.SetStasis(true),
            (id) => input.target.RemoveFromBuffs(id),
            () => input.target.SetStasis(false)
            );
        BuffTarget(input, effect);
    }

    void SpeedUpMult(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => input.target.ModifyMSMult(input.magnitude),
            (id) => input.target.RemoveFromBuffs(id),
            () => input.target.ModifyMSMult(-input.magnitude)
            );
        BuffTarget(input, effect);
    }

    void SpeedUpFlat(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => input.target.ModifyMSFlat(input.magnitude),
            (id) => input.target.RemoveFromBuffs(id),
            () => input.target.ModifyMSFlat(-input.magnitude)
            );
        BuffTarget(input, effect);
    }

    void Cleanse(AbilityInput input)
    {
        input.target.RemoveAllDebuffs();
    }

    void Dash(AbilityInput input)
    {
        //use the mover/move from playercontroller after giving them parameters
        input.target.Dash(input);
    }
    #endregion

    #region Enemy Abilities
    void Damage(AbilityInput input)
    {
        if(input.duration == 0)//instant damage 
        {
            input.target.ApplyDamage(input.magnitude);
        }
        else//damage over time, untested
        {
            //MAYBE add a new variable called stackable and if the ability is stackable dont use this block, if its not stackable use the part etc etc
            Effect tickingEffect = new Effect(
                Effect.Type.Ticking,
                () => input.target.ApplyDamage(input.magnitude),
                (id) => input.target.RemoveFromDebuffs(id)
                );
            DebuffTarget(input, tickingEffect);
        }
    }

    void Stun(AbilityInput input)//untested
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => input.target.SetStunned(true),
            (id) => input.target.RemoveFromDebuffs(id),
            () => input.target.SetStunned(false)
            );
        DebuffTarget(input, effect);
    }

    void Root(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => input.target.SetRooted(true),
            (id) => input.target.RemoveFromDebuffs(id),
            () => input.target.SetRooted(false)
            );
        DebuffTarget(input, effect);
    }

    void Mark(AbilityInput input)//untested
    {
        /*
        Effect effect = new Effect(
            Effect.Type.Status,
            () => playerController.Mark(input, true),
            (id) => playerController.RemoveFromDebuffs(id),
            () => playerController.Mark(input, false)
            );
        DebuffTarget(input, effect);
        */
    }

    void SlowFlat(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => input.target.ModifyMSFlat(-input.magnitude),
            (id) => input.target.RemoveFromDebuffs(id),
            () => input.target.ModifyMSFlat(input.magnitude)
            );
        DebuffTarget(input, effect);
    }

    void SlowMult(AbilityInput input)
    {
        Effect effect = new Effect(
            Effect.Type.Status,
            () => input.target.ModifyMSMult(input.magnitude),
            (id) => input.target.RemoveFromDebuffs(id),
            () => input.target.ModifyMSMult(1 / input.magnitude)
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
        //effect.Undo();
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
        EffectInfo buffInfo = input.target.HasBuff(input.id);
        
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

        buffInfo = new EffectInfo(buffCoroutine, effect, input.source);
        input.target.AddToBuffs(input.id, buffInfo);
    }

    void DebuffTarget(AbilityInput input, Effect effect)
    {
        EffectInfo debuffInfo = input.target.HasDebuff(input.id);

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

        debuffInfo = new EffectInfo(coroutine, effect, input.source);
        input.target.AddToDebuffs(input.id, debuffInfo);
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
        
        [SerializeField]
        AbilityComponentData.Type type;
        public AbilityComponentData.Type CompType
        {
            get { return type; }
        }

        public void Apply(string id, ICharacter target, GameObject source)
        {
            //Debug.Log("Applying Ability Component: " + type.ToString() +" Magnitude: " + magnitude.ToString() + " Duration: " + duration.ToString());
            componentDictionary[type].Invoke(new AbilityInput(target, source, id, magnitude, duration));
        }

        public void ForceInvoke(string id, ICharacter target, GameObject source)
        {
            componentDictionary[type].Invoke(new AbilityInput(target, source, id, magnitude, duration));
        }
    }

    public struct AbilityComponentData
    {
        public enum Type { Damage, Stun, Root, Mark, SlowFlat, Heal, SpeedUpFlat, Blink, Stasis, Cleanse, Dash, SlowMult, SpeedUpMult };

        [HideInInspector]
        public static readonly HashSet<Type> invokeEachTick = new HashSet<Type> { Type.Damage, Type.Heal };
        [HideInInspector]
        public static readonly HashSet<Type> statuses = new HashSet<Type> { Type.Stun, Type.Root, Type.Mark, Type.Stasis };
        [HideInInspector]
        public static readonly HashSet<Type> statFlatModifiers = new HashSet<Type> { Type.SlowFlat, Type.SpeedUpFlat };
        [HideInInspector]
        public static readonly HashSet<Type> statMultModifiers = new HashSet<Type> { Type.SlowMult, Type.SpeedUpMult };

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
        public GameObject source;
        public EffectInfo(Coroutine crt, Effect eff, GameObject src)
        {
            coroutine = crt;
            effect = eff;
            source = src;
        }
    }

    class ComponentActionPair
    {
        public AbilityComponentData.Type componentType;
        public Action function;
    }

    public class AbilityInput
    {
        public string id = "";
        public float magnitude = 0, duration = 0;
        public GameObject source;
        public ICharacter target;

        public AbilityInput(ICharacter _target, GameObject _source, string _id = "", float _magnitude = 0, float _duration = 0)
        {
            id = _id;
            magnitude = _magnitude;
            duration = _duration;
            target = _target;
            source = _source;
        }
    }
    [Serializable]//, RequireComponent(typeof(Collider2D))] reqComp doesnt work here apparently, sadly
    public class Targeting
    {
        public enum Type { TargetGround, Projectile , UnitSpawn, Laser, Self };//this is unused
        public Type type;//this is unused 
        public string targetingLayer;
        public LayerMask LayerMask
        {
            get { return targetingLayer != "" ? LayerMask.GetMask(targetingLayer) : default(LayerMask); }
        }
        public GameObject collidingUnit;//unit that has a collider such as a projectile

    }

    #endregion

}
