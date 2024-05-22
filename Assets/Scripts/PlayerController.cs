using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    int baseMovementSpeed, health;
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
            mousePosition.z = 0;
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

    Dictionary<string, Coroutine> buffs, debuffs;

    bool rooted, stunned, stasis;//states

    List<string> markedBy;

    public static PlayerController Instance;


    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public CommandIndicator commandIndicator;

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

        SetUpInputs();
        InitStats();
    }

    // Update is called once per frame
    void Update()
    {
    }
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
    public void Dash()
    {

    }
    

    #region Movement Functions
    void Move()//modify this with parameters so this can be used for dashes too
    {
        if (rooted || stunned)
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
        //Debug.Log(targetPosition);

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
        float tStep = 0.01f / (distance / movementSpeed);
        while ((transform.position - targetPosition).magnitude > stopDistance)
        {
            t += tStep;
            transform.position = Vector3.Lerp(oldPosition, targetPosition, t);//lerp already clamps so no need to mathfMin it
            yield return new WaitForFixedUpdate();
        }
    }
    #endregion
    #endregion

    #region Player Stat Interactions
    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void ApplyDamage(AbilityManager.AbilityInput input)
    {
        currentHealth -= input.magnitude;
    }

    public void ApplyHeal(int heal)
    {
        currentHealth = Mathf.Min(health, currentHealth + heal);
    }

    public void ApplyHeal(AbilityManager.AbilityInput input)
    {
        currentHealth = Mathf.Min(health, currentHealth + input.magnitude);
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
        msModifier += isPositive ? input.magnitude : -input.magnitude;
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

    public void RemoveAllDebuffs()
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

    



    #endregion
}
