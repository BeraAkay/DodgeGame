using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    PlayerVisuals playerVisualManager;

    [SerializeField]
    int baseMovementSpeed, health;
    [SerializeField]
    float currentHealth, movementSpeed;

    float msModifier;

    [SerializeField]
    float stopDistance;
    public float Health
    {
        get { return currentHealth; }
    }

    Vector3 mousePosition;
    public Vector3 MousePosition
    {
        get
        {
            mousePosition = Camera.main.ScreenToWorldPoint(playerInput.actions["TargetPosition"].ReadValue<Vector2>());
            mousePosition.z = 0;//might need to change this to player.transform.z in the future if need be
            return mousePosition;
        }
    }

    [SerializeField]
    Ability ability, special1, special2;

    Coroutine moveCoroutine;
    Vector3 targetPosition;

    Vector3 oldPosition;

    PlayerInput playerInput;
    InputActionMap actionMap;

    Dictionary<string, Coroutine> buffs, debuffs;//change the coroutine into effectovertime structs that have their duration editable etc so its more oop and less spaghetti

    public List<string> effectList;

    bool rooted, stunned, stasis, dashing;//player states

    public bool invincible, god;

    List<string> markedBy;//wont be used at this rate

    public static PlayerController Instance;


    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public CommandIndicator commandIndicator;

    CinemachineImpulseSource impulseSource;

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        playerVisualManager = GetComponent<PlayerVisuals>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        SetUpInputs();
        InitStats();



        StartCoroutine(ListUpdater());
        Debug.Log("Conversion Rate: " + GameManager.distanceConversionRate);
        Debug.Log("Movement Update Rate: " + GameManager.movementUpdateRate);
    }

    // Update is called once per frame
    void Update()
    {
    }

    #region Debug Functions
    void UpdateEffectList()
    {
        effectList = new List<string>();
        foreach(string key in buffs.Keys)
        {
            effectList.Add(key);
        }
        foreach(string key in debuffs.Keys)
        {
            effectList.Add(key);
        }
    }
    IEnumerator ListUpdater()
    {
        while (true)
        {
            UpdateEffectList();
            yield return new WaitForSeconds(0.25f);
        }
    }

    #endregion

    #region Initialization Functions
    void SetUpInputs()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["MoveCommand"].performed += ctx => Move();
        playerInput.actions["UseAbility"].performed += ctx => ability.Use();
        playerInput.actions["UseSpecial1"].performed += ctx => special1.Use();
        playerInput.actions["UseSpecial2"].performed += ctx => special2.Use();

    }
    
    void InitStats()
    {
        movementSpeed = baseMovementSpeed;
        currentHealth = health;
        rooted = stunned = stasis = false;
        buffs = new Dictionary<string, Coroutine>();
        debuffs = new Dictionary<string, Coroutine>();
        markedBy = new List<string>();
    }
    #endregion

    #region Player Action Functions
    public void Dash(AbilityManager.AbilityInput input)
    {
        /*
         * input.magnitude = > Distance (needs to be converted)
         * input.duration = > travelTime
         */
        if (rooted || stunned || stasis || dashing)
        {
            return;
        }
        dashing = true;

        targetPosition = MousePosition;
        oldPosition = transform.position;
        targetPosition = Vector3.Lerp(oldPosition, targetPosition, (GameManager.distanceConversionRate * input.magnitude)/Vector3.Distance(oldPosition, targetPosition));

        Debug.DrawLine(oldPosition, targetPosition, Color.green, input.duration);

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(Dasher(input));
    }
    

    #region Movement Functions
    void Move()//modify this with parameters so this can be used for dashes too
    {
        if (rooted || stunned || stasis || dashing)
        {
            return;
        }

        /*
        targetPosition = playerInput.actions["TargetPosition"].ReadValue<Vector2>();
        targetPosition = Camera.main.ScreenToWorldPoint(targetPosition);

        targetPosition.z = 0;
        */
        targetPosition = MousePosition;
        commandIndicator.IndicateLocation(targetPosition);

        oldPosition = transform.position;

        //start lerp to target position
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(Mover());
    }


    IEnumerator Mover()//modify this with parameters so this can be used for dashes too
    {
        float distance = Vector3.Distance(oldPosition, targetPosition);
        float t = 0;
        float tStep = (movementSpeed * GameManager.distanceConversionRate) / distance;//RAW tStep, needs to be multed by elapsed time each wait since its not reliable
        float start = Time.time;
        while ((transform.position - targetPosition).magnitude > stopDistance)
        {
            t += tStep * Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(oldPosition, targetPosition, t);//lerp already clamps so no need to mathfMin it
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Distance: " + Vector3.Distance(oldPosition, transform.position) + " Time: " + (Time.time - start));
    }

    IEnumerator Dasher(AbilityManager.AbilityInput input)
    {
        float t = 0;
        float tStep = 1 / input.duration;//RAW tStep, needs to be multed by elapsed time each wait since its not reliable
        float start = Time.time;
        while (t < 1)
        {
            t += tStep * Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(oldPosition, targetPosition, t);//lerp already clamps so no need to mathfMin it
            yield return new WaitForFixedUpdate();
        }
        dashing = false;
        Debug.Log("Distance: " + Vector3.Distance(oldPosition, transform.position) + " Time: " + (Time.time - start));
    }
    #endregion
    #endregion

    #region Player Stat Interactions
    public void ApplyDamage(float damage)
    {
        if(currentHealth <= 0 || stasis || invincible || god)
        {
            return;
        }

        playerVisualManager.HitReaction();
        playerVisualManager.FlashColor(Color.red);
        CameraShaker.instance.Shake(impulseSource);
        currentHealth -= damage;

        UIManager.instance.SetHealthBarFill(currentHealth / health);

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyDamage(AbilityManager.AbilityInput input)
    {
        ApplyDamage(input.magnitude);
    }

    public void ApplyHeal(float heal)
    {
        if (stasis)
        {
            return;
        }

        playerVisualManager.FlashColor(Color.green);
        currentHealth = Mathf.Min(health, currentHealth + heal);


        UIManager.instance.SetHealthBarFill(currentHealth / health);
    }

    public void ApplyHeal(AbilityManager.AbilityInput input)
    {
        ApplyHeal(input.magnitude);
    }

    /*
    public void HealthChange(AbilityManager.AbilityInput input, bool isPositive)
    {
        if (isPositive)
        {
            ApplyHeal(input.magnitude);
        }
        else
        {
            ApplyDamage(input.magnitude);
        }
    }
    */
    public void ModifyMovementSpeed(AbilityManager.AbilityInput input, bool isPositive)
    {
        msModifier += isPositive ? input.magnitude : -input.magnitude;//add a mult/percentage version for this.
        movementSpeed = msModifier + baseMovementSpeed;
    }

    public void Root(bool flag)
    {
        if (flag && moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        rooted = flag;
    }

    public void Stun(bool flag)
    {
        if (flag)
        {
            StopCoroutine(moveCoroutine);
        }
        stunned = flag;
    }

    public void Stasis(bool flag)
    {
        if (flag)
        {
            StopCoroutine(moveCoroutine);
        }
        stasis = flag;
    }

    public void Mark(AbilityManager.AbilityInput input, bool flag)
    {
        if (flag)
        {
            markedBy.Add(input.id);

            Debug.Log("Player Marked");
        }
        else
        {
            markedBy.Remove(input.id);
        }
    }

    public void RemoveAllDebuffs()//TODO: fix this or call this with a cleansing func
    {
        foreach(string key in debuffs.Keys)
        {
            Coroutine crt = debuffs[key];
            if(crt != null)
            {
                StopCoroutine(crt);
            }
            debuffs.Remove(key);
        }
    }

    public void AddToDebuffs(string id, Coroutine debuffCoroutine)
    {
        debuffs.Add(id, debuffCoroutine);
    }

    public void RemoveFromDebuffs(string id)
    {
        debuffs.Remove(id);
    }

    public Coroutine HasDebuff(string id)
    {
        return debuffs.ContainsKey(id) ? debuffs[id] : null;
    }

    public void AddToBuffs(string id, Coroutine buffCoroutine)
    {
        buffs.Add(id, buffCoroutine);
    }

    public void RemoveFromBuffs(string id)
    {
        buffs.Remove(id);
    }

    public Coroutine HasBuff(string id)
    {
        return buffs.ContainsKey(id) ? buffs[id] : null;
    }

    void Die()
    {
        GameManager.instance.PlayerDeath();
    }


    #endregion


    public struct EffectOverTime
    {
        public string id;
        public Coroutine coroutine;
        

    }

}
