using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRangeStateMachine : MonoBehaviour
{
    public enum MonsterState
    {
        Idle,
        Moving,
        Attacking
    }

    public MonsterState currentState;

    public float detectionRange = 6;
    public LayerMask defenderLayer;
    public LayerMask towerLayer;
    public float coolDown = 2.5f; // 攻击冷却时间
    private float attackTimer; // 冷却计时器
    Vector3 offset = new Vector3(0, 0.7f, 0); // 这里的 1 表示向上偏移 1 个单位

    private MonsterMovement monsterMovement;
    private MonsterAttackRange monsterAttackRange;




    void Start()
    {
        attackTimer = 0;

        monsterMovement = GetComponent<MonsterMovement>();
        monsterAttackRange = GetComponent<MonsterAttackRange>();

        //Debug.Log($"MonsterMovement: {monsterMovement}, MonsterAttackRange: {monsterAttackRange}");
        //Debug.Log($"MonsterMovement: {monsterMovement}, Enabled: {monsterMovement.enabled}, MonsterAttackRange: {monsterAttackRange}, Enabled: {monsterAttackRange.enabled}");



        if (monsterMovement == null || monsterAttackRange == null)
        {
            Debug.LogError($"Missing components! MonsterMovement: {monsterMovement}, MonsterAttackRange: {monsterAttackRange}");
            enabled = false; // 禁用脚本以防止后续报错
            return;
        }

        Debug.Log("Components initialized successfully.");
        TransitionToState(MonsterState.Idle);
    }

    void Update()
    {
        // 更新冷却计时器（无论当前状态如何都应运行）
        if(attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        switch (currentState)
        {
            case MonsterState.Idle:
                HandleIdleState();
                break;
            case MonsterState.Moving:
                HandleMovingState();
                break;
            case MonsterState.Attacking:
                HandleAttackingState();
                break;
        }
        //Debug.Log($"Transitioning to state: {currentState}");
    }

    private void TransitionToState(MonsterState newState)
    {
        // Debug log for state transition
        //Debug.Log($"Transitioning from {currentState} to {newState}");
        currentState = newState;



    }

    private void HandleIdleState()
    {
        // Idle 状态的逻辑可以省略，直接切换到 Moving
        TransitionToState(MonsterState.Moving);
    }

    private void HandleMovingState()
    {
        // 启用移动
        monsterMovement.EnableMovement(true);


        // 检测目标是否在范围内
        if (DetectDefender() && attackTimer <= 0)
        {
            TransitionToState(MonsterState.Attacking);
        }
    }

    private void HandleAttackingState()
    {
        // 启用移动
        monsterMovement.EnableMovement(false);

        if (attackTimer <= 0)
        {
            if (DetectDefender())
            {
                // 如果探测到目标，执行攻击
                monsterAttackRange.TriggerRangedAttack();
                attackTimer = coolDown; // 重置冷却计时器
            }
            else
            {
                // 如果未探测到目标，切换回移动状态
                TransitionToState(MonsterState.Moving);
            }
        }
        else
        {
            // 如果冷却中但未探测到目标，切换回移动状态
            if (!DetectDefender())
            {
                TransitionToState(MonsterState.Moving);
            }
        }

    }

    private bool DetectDefender()
    {
        // 合并 defenderLayer 和 towerLayer
        int combinedLayerMask = defenderLayer | towerLayer; // 使用按位或操作符 | 合并 LayerMask

        // 发射射线检测目标
        RaycastHit2D hit = Physics2D.Raycast(transform.position + offset, Vector2.left, detectionRange, combinedLayerMask);
        Debug.DrawRay(transform.position + offset, Vector2.left * detectionRange, Color.red);
        if (hit.collider != null)
        {
            //Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
        }
        else
        {
            //Debug.Log("Raycast missed.");
        }

        return hit.collider != null; // 如果检测到目标，返回 true，否则返回 false
    }
}
