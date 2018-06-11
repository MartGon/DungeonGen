using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

    // Enemy Info
    public string unitName;
    public int level;

    // Level 1 stats
    public float baseHealthPoints;
    public float baseDamage;
    public float baseJumpForce;
    public float baseAtttackRate;
    public float baseCombatDistance;
    public float baseRecoverTimer;

    // stats
    public float currentHealthPoints;
    public float speedRecoverRate;
    protected float combatDistance;
    protected float healthPoints;
    protected float damage;

    // Misc
    public Room homeRoom;
    public GameObject ammoBox;
    public GameObject hpBox;
    protected GameObject Player;
    protected NavMeshAgent navMeshAgent;

    // Drop Chance
    public float ammoDropChance = 0.15f;
    public float hpDropChance = 0.05f;

    // Movement
    protected float speedRecoverCount;
    protected float distanceToPlayer;
    protected float movementSpeed = 0;

    // Components
    public Animator animator;
    public BoxCollider boxCollider;
    public Rigidbody unitRigidbody;

    // UI
    public UnitUI unitUI;

    // State
    public enum UnitState
    {
        DEAD = 1,
        RECOVERING = 2,
        IDLE = 4,
        IN_COMBAT = 8,
        ATTACKING = 16,
        ON_ATTACK = 32,
    }
    public UnitState state;
    public UnitSubState subState;

    public enum UnitSubState
    {
        IDLE_NONE,
        IDLE_MOVING,
        IDLE_STOPPED,
        IDLE_LEFT_COMBAT
    }

        // IDLE state
    Vector3 patrolPosition;

    // Recover state
    protected float recoverTimer;
    protected float recoverTimerCount;

    // In_Combat state 
    protected float attackRateCount;
    protected float attackRate;

    public float leaveCombatTimer;
    protected float leaveCombatTimerCount;

    // Attackin state
    protected float attackCastLenght;
    protected float attackCastLenghCount;
    protected Vector3 attackDirection;

    // On Attack
    protected float jumpForce = 1f;

    // Use this for initialization
    void Start ()
    {
        // State
        state = UnitState.IDLE;
        subState = UnitSubState.IDLE_STOPPED;

        // Stats
        getLevelByRound();
        generateStatsByLevel();
        currentHealthPoints = healthPoints;

        // Timers
        recoverTimerCount = recoverTimer;
        speedRecoverCount = speedRecoverRate;

        // Position
        patrolPosition = transform.position;

        // Unit UI
        unitUI.setName(unitName);
        unitUI.setLevel(level);
        unitUI.maxHealthPoints = healthPoints;
        unitUI.updateHealthPoints(currentHealthPoints);

        // Components
        Player = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        movementSpeed = navMeshAgent.speed;
	}
	
	// Update is called once per frame
	void Update ()
    {
        updateState();
        checkBug();
	}

    void updateState()
    {
        if(state != UnitState.RECOVERING)
            checkDeath();

        switch(state)
        {
            case UnitState.IDLE:
                unitUI.gameObject.SetActive(false);

                switch(subState)
                {
                    case UnitSubState.IDLE_LEFT_COMBAT:
                        goToPosition(patrolPosition);
                        subState = UnitSubState.IDLE_MOVING;
                        break;

                    case UnitSubState.IDLE_STOPPED:
                        if (homeRoom == null)
                            break;

                        Vector3 position = homeRoom.getRandomPosition();
                        goToPosition(position);

                        subState = UnitSubState.IDLE_MOVING;
                        break;
                    case UnitSubState.IDLE_MOVING:
                        if (navMeshAgent.remainingDistance < 5)
                        {
                            subState = UnitSubState.IDLE_STOPPED;
                        }
                        if(navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
                        {
                            Debug.Log("Path inválido");
                            subState = UnitSubState.IDLE_STOPPED;
                        }
                        break;  
                }

                if (canSeePlayer())
                    state = UnitState.IN_COMBAT;

                break;
            case UnitState.IN_COMBAT:
                if (!canSeePlayer())
                {
                    leaveCombatTimerCount -= Time.deltaTime;
                    if (leaveCombatTimerCount < 0)
                    {
                        state = UnitState.IDLE;
                        subState = UnitSubState.IDLE_LEFT_COMBAT;
                        leaveCombatTimerCount = leaveCombatTimer;
                    }
                }
                else
                    leaveCombatTimerCount = leaveCombatTimer;

                if(isInCombatDistance())
                {
                    if (isAttackReady())
                        startAttack();
                    else
                        stopChasing();
                }
                else
                    chasePlayer();

                updateAttackTimer();
                break;
            case UnitState.ATTACKING:
                if (attackAnimationIsOver())
                {
                    performAttack();
                }
                break;
            case UnitState.ON_ATTACK:
                break;
            case UnitState.DEAD:
                unitUI.gameObject.SetActive(false);

                // Check item drops
                checkDrops();

                // Disable flags
                navMeshAgent.enabled = false;
                animator.SetBool("Attack", false);
                animator.SetBool("MoveForward", false);
                animator.SetBool("Death", true);
                unitRigidbody.isKinematic = false;
                gameObject.layer = 9;

                state = UnitState.RECOVERING;

                break;
            case UnitState.RECOVERING:
                recover();
                break;
        }
    }

    bool checkDeath()
    {
        bool dead = currentHealthPoints < 0;
        if (dead)
            state = UnitState.DEAD;
        return dead;
    }

    bool checkBug()
    {
        bool result = transform.position.y < -20;
        if (result)
        {
            gameObject.SetActive(false);
        }
        return result;
    }

    public float reduceHPByAmount(float amount)
    {
        unitUI.updateHealthPoints(currentHealthPoints - amount);
        return currentHealthPoints -= amount;
    }

    bool canSeePlayer()
    {
        bool result = castRayToGameObject(Player);
        if (result)
            unitUI.gameObject.SetActive(true);
        else
            unitUI.gameObject.SetActive(false);

        return result;
    }

    bool isAttackReady()
    {
        return attackRateCount < 0;
    }

    bool isInCombatDistance()
    {
        distanceToPlayer = (gameObject.transform.position - Player.transform.position).magnitude;
        return distanceToPlayer < combatDistance;
    }

    void chasePlayer()
    {
        navMeshAgent.enabled = true;
        navMeshAgent.SetDestination(Player.transform.position);
        animator.SetBool("MoveForward", true);
    }

    protected void stopChasing()
    {
        navMeshAgent.enabled = false;
        animator.SetBool("MoveForward", false);
    }

    bool goToPosition(Vector3 position)
    {
        navMeshAgent.enabled = true;
        animator.SetBool("MoveForward", true);
        navMeshAgent.SetDestination(position);
        NavMeshPath navMeshPath = new NavMeshPath();

        navMeshAgent.CalculatePath(position, navMeshPath);

        return navMeshPath.status == NavMeshPathStatus.PathComplete;
    }

    void updateAttackTimer()
    {
        attackRateCount -= Time.deltaTime;
    }

    bool attackAnimationIsOver()
    {
        attackCastLenghCount -= Time.deltaTime;

        return attackCastLenghCount < 0;
    }

    public void reduceMovementSpeed()
    {
        float speed = navMeshAgent.speed;

        if (speed < movementSpeed / 10)
            return;

        speed *= 0.99f;
        navMeshAgent.speed = speed;
    }

    public void recoverMovementSpeed()
    {
        if (speedRecoverCount <= 0)
        {
            float speed = navMeshAgent.speed;

            if (speed >= movementSpeed)
                return;

            speed *= 1.1f;
            navMeshAgent.speed = speed;
            speedRecoverCount = speedRecoverRate;
        }
        else
            speedRecoverCount -= Time.deltaTime;
    }

    public virtual void startAttack()
    {
        // Stop following
        stopChasing();

        // Animation
        animator.SetBool("Attack", true);

        // Attack Rate
        attackRateCount = attackRate;

        attackCastLenghCount = Utilities.getAnimationLengthByNameInAnimator("PA_WarriorAttack_Clip", animator)/4;

        // Saving attack position
        Vector3 playerPosition = Player.transform.position + new Vector3(0, 1, 0);
        attackDirection = playerPosition - transform.position;

        state = UnitState.ATTACKING;
    }

    public void dealDamageToPlayer()
    {
        Player playerObject = Player.GetComponent<Player>();

        if (playerObject)
        {
            playerObject.receiveDamage(damage);
        }
    }

    public void checkDrops()
    {
        float check = Random.Range(0f, 1f);
        Vector3 heightOffset = new Vector3(0, 1, 1);

        GameObject item;
        if(check < hpDropChance)
        {
            item = GameObject.Instantiate(hpBox, gameObject.transform.position + heightOffset, Quaternion.identity);
            Rigidbody r = item.GetComponent<Rigidbody>();
            if (r)
                r.AddForce(heightOffset * 5, ForceMode.Impulse);
            return;
        }

        if(check < ammoDropChance)
        {
            item = GameObject.Instantiate(ammoBox, gameObject.transform.position + heightOffset, Quaternion.identity);
            Rigidbody r = item.GetComponent<Rigidbody>();
            if (r)
                r.AddForce(heightOffset * 5, ForceMode.Impulse);
            return;
        }
    }

    public void recover()
    {
        recoverTimerCount -= Time.deltaTime;
        if (recoverTimerCount < 0)
        {
            // Enables/disables
            navMeshAgent.enabled = true;
            animator.SetBool("Death", false);
            //boxCollider.enabled = true;
            gameObject.layer = 8;

            // Reset health and timer
            recoverTimerCount = recoverTimer;
            currentHealthPoints = healthPoints/2;
            unitUI.updateHealthPoints(currentHealthPoints);
            unitRigidbody.isKinematic = true;

            state = UnitState.IDLE;
            subState = UnitSubState.IDLE_LEFT_COMBAT;
        }
    }

    public virtual void performAttack()
    {
        animator.SetBool("Attack", false);
        navMeshAgent.enabled = false;
        unitRigidbody.isKinematic = false;

        unitRigidbody.AddForce(attackDirection.normalized * jumpForce, ForceMode.Impulse);
        state = UnitState.ON_ATTACK;
    }

    public void getLevelByRound()
    {
        int round = PlayerPrefs.GetInt("level");
        level = Random.Range(round - 2, round + 2);

        if (level <= 0)
            level = 1;
    }

    public virtual void generateStatsByLevel()
    {
        healthPoints = baseHealthPoints + 10 * level;
        damage = baseDamage + 2 * level;
        jumpForce = baseJumpForce + 1 * level;
        attackRate = baseAtttackRate - 0.05f * level;
        combatDistance = baseCombatDistance + 0.25f * level;
        recoverTimer = baseRecoverTimer - 2.5f * level;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (state != UnitState.ON_ATTACK)
            return;

        if(other.GetComponent<Player>())
        {
            Player playerObject = Player.GetComponent<Player>();
            playerObject.receiveDamage(damage);
            attackRateCount = attackRate;
        }

        navMeshAgent.enabled = true;
        unitRigidbody.isKinematic = true;
        state = UnitState.IN_COMBAT;
    }

    protected bool castRayToGameObject(GameObject target)
    {
        Vector3 origin = transform.position;
        Vector3 direction = target.transform.position - origin;
        RaycastHit info;

        // Layer phy stuff
        int enemyLayer = 1 << 8;
        int deadEnemyLayer = 1 << 9;
        int layerMask = enemyLayer | deadEnemyLayer;
        layerMask = ~layerMask;

        Physics.Raycast(origin, direction, out info, Mathf.Infinity, layerMask);

        return info.transform.gameObject == target;
    }

    protected RaycastHit castRayTowards(Vector3 direction)
    {
        Vector3 origin = transform.position;
        RaycastHit info;

        // Layer phy stuff
        int enemyLayer = 1 << 8;
        int deadEnemyLayer = 1 << 9;
        int layerMask = enemyLayer | deadEnemyLayer;
        layerMask = ~layerMask;

        Physics.Raycast(origin, direction, out info, Mathf.Infinity, layerMask);
        Debug.DrawRay(origin, direction);

        return info;
    }
}
