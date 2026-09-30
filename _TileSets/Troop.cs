//using MyLib.Combat;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Troop : MonoBehaviour {
//    [SerializeField] private TroopData data;
//    [SerializeField] private Health healthComponent;

//    private TroopState state;

//    private ContactFilter2D enemyFilter;
//    private Collider2D[] detectionResults;

//    private IDamageable currentTarget;
//    private TroopPool myPool;

//    private LayerMask enemyLayer;
//    private float moveDirection = 1f;

//    private int currentAttackCooldown;
//    private int currentAttackDamage;
//    private int currentAttackRange;
//    private int currentMoveSpeed;

//    private List<StatModifier> damageModifiers;

//    private const int MinHealth = 0;

//    private float attackTimer;

//    private void Awake() {
//        damageModifiers = new List<StatModifier>();
//        detectionResults = new Collider2D[5];

//        enemyFilter = new ContactFilter2D { useLayerMask = true };
//        enemyFilter.SetLayerMask(enemyLayer);
//        enemyFilter.useTriggers = false;
//    }

//    private void OnEnable() {
//        state = TroopState.Idle;
//        healthComponent.OnDeath.AddListener(HandleDeath);

//        currentAttackCooldown = data.AttackCooldown;
//        currentAttackDamage = data.AttackDamage;
//        currentAttackRange = data.AttackRange;
//        currentMoveSpeed = data.MoveSpeed;
//    }

//    private void Update() {
//        switch (state) {
//            case TroopState.Idle:
//                // Todo: Wait for animation
//                state = TroopState.Moving;
//                break;
//            case TroopState.Moving:
//                if (DetectEnemies()) state = TroopState.Attacking;
//                else { // Move the troop
//                    Vector3 moveDir = new Vector3(moveDirection, 0, 0);
//                    transform.Translate(moveDir * currentMoveSpeed * Time.deltaTime);
//                }
//                break;
//            case TroopState.Attacking:
//                if (currentTarget == null || !currentTarget.IsTargetable) {
//                    currentTarget = null;
//                    state = TroopState.Moving;
//                }
//                else {
//                    attackTimer -= Time.deltaTime;
//                    if (attackTimer <= 0) {
//                        currentTarget?.DamageTaken(currentAttackDamage);

//                        attackTimer = currentAttackCooldown;
//                    }
//                }
//                break;
//            case TroopState.Dead:
//                break;
//        }
//    }

//    public IEnumerator AfterDeathRoutine() {
//        yield return new WaitForSeconds(3);
//        FinishDeath();
//    }

//    public void FinishDeath() {
//        //myPool.ReturnToPool(this);
//    }

//    public void Initialize(TroopPool pool, LayerMask targetLayerMask, float dir) {
//        myPool = pool;
//        enemyLayer = targetLayerMask;
//        moveDirection = dir;

//        enemyFilter.SetLayerMask(enemyLayer);

//        SpriteRenderer mySprite = GetComponentInChildren<SpriteRenderer>();
//        if (mySprite != null) {
//            mySprite.flipX = (moveDirection < 0);
//        }
//    }

//    private int RecalculateStat(int baseStat, List<StatModifier> statModifiers) {
//        int newStat = baseStat;
//        foreach (StatModifier statModifier in statModifiers) {
//            if (statModifier.modifierType == ModifierType.FlatAddition) {
//                newStat += statModifier.value;
//            } else if (statModifier.modifierType == ModifierType.PercentageMultiplier) {
//                newStat += (int)((float)statModifier.value * baseStat / 100);
//            }
//        }
//        return newStat;
//    }

//    private bool DetectEnemies() {
//        int count = Physics2D.OverlapCircle(transform.position, currentAttackRange, enemyFilter, detectionResults);
//        Debug.Log($"Troop.DetectEnemies.count = {count}");
//        IDamageable closestTarget = null;
//        float minDistance = float.MaxValue;
//        for (int i = 0; i < count; i++) {
//            IDamageable potentialTarget = detectionResults[i].GetComponent<IDamageable>();
//            Debug.Log($"Troop.DetectEnemies.potentialTarget:{potentialTarget}");
//            if (potentialTarget != null) {
//                float currDistance = Vector2.Distance(transform.position, detectionResults[i].transform.position);
//                if (currDistance < minDistance) {
//                    minDistance = currDistance;
//                    closestTarget = potentialTarget;
//                }
//            }
//        }

//        if (closestTarget != null) {
//            currentTarget = closestTarget;
//            Debug.Log($"Troop.DetectEnemies.currentTarget:{currentTarget}");
//            return true;
//        }
//        return false;
//    }

//    private void HandleDeath() {
//        state = TroopState.Dead;
//        StartCoroutine(AfterDeathRoutine());
//    }

//    private void OnDisable() {
//        healthComponent.OnDeath.RemoveListener(HandleDeath);
//        StopAllCoroutines();
//    }
//}
