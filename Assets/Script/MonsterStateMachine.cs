using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStateMachine : MonoBehaviour
{
    public enum MonsterState
    {
        Idle,
        Moving,
        Attacking
    }

    public MonsterState currentState;

    private MonsterMovement monsterMovement;
    private MonsterAttack monsterAttack;

    public float idleTime = 1.0f; // 为怪物在攻击或停止后提供一个短暂的等待时间，使行为看起来更自然
    private float idleTimer;

    private bool isNearTarget = false;

    void Start()
    {
        // Get references to MonsterMovement and MonsterAttack
        monsterMovement = GetComponent<MonsterMovement>();
        monsterAttack = GetComponent<MonsterAttack>();

        //Debug.Log($"MonsterMovement: {monsterMovement}, MonsterAttackRange: {monsterAttack}");
        //Debug.Log($"MonsterMovement: {monsterMovement}, Enabled: {monsterMovement.enabled}, MonsterAttackRange: {monsterAttack}, Enabled: {monsterAttack.enabled}");



        if (monsterMovement == null || monsterAttack == null)
        {
            Debug.LogError($"Missing components! MonsterMovement: {monsterMovement}, MonsterAttackRange: {monsterAttack}");
            enabled = false; // 禁用脚本以防止后续报错
            return;
        }

        //Debug.Log("Components initialized successfully.");
        TransitionToState(MonsterState.Idle);
    }

    void Update()
    {
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
    }

    private void TransitionToState(MonsterState newState)
    {
        // Debug log for state transition
        //Debug.Log($"Transitioning from {currentState} to {newState}");
        currentState = newState;

        // Reset timers or state-spicific variables here
        //if(newState == MonsterState.Idle)
        //{
        //    idleTimer= idleTime;
        //}
    }

    private void HandleIdleState()
    {
        // Idle 状态的逻辑可以省略，直接切换到 Moving
        TransitionToState(MonsterState.Moving);
    }

    private void HandleMovingState()
    {
        //Enable movement
        monsterMovement.enabled = true;

        if (isNearTarget )
        {
            TransitionToState(MonsterState.Attacking);
        }
    }

    private void HandleAttackingState()
    {
        monsterMovement.enabled = false;

        // Attack logic handled by MonsterAttack
        if (!monsterAttack.enableAreaAttack)
        {
            TransitionToState(MonsterState.Moving);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detect if near a target (Defender, Tower, etc.)
        if (collision.CompareTag("Defender") || collision.CompareTag("Tower") || collision.CompareTag("Melee"))
        {
            isNearTarget = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // No longer near a target
        if (collision.CompareTag("Defender") || collision.CompareTag("Tower") || collision.CompareTag("Melee"))
        {
            isNearTarget = false;
            TransitionToState(MonsterState.Moving); // Resume moving if the target is no longer nearby
        }
    }
}


