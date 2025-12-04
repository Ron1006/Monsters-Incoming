using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherStatementMachine : MonoBehaviour
{
    public ArcherState archerState;
    public float detectionRange = 6;
    public LayerMask enemyLayer;
    public ArcherMovement archerMovement;
    public ArcherMan archerMan;
    public float coolDown = 2.5f;
    private float timer;
    Vector3 offset = new Vector3(0, 0.7f, 0); // 这里的 1 表示向上偏移 1 个单位

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        archerState = ArcherState.Walking;
        archerMan.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //detectEnemy();
        // 冷却计时器倒计时
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (archerState == ArcherState.Walking)
        {
            if (detectEnemy() && timer <= 0)
            {
                // 切换到射击状态
                archerState = ArcherState.Shooting;
                archerMan.enabled = true;
                archerMan.Attack();

                //Debug.Log("检测到敌人，并冷却结束，停止移动并攻击");

                // 重置冷却时间
                timer = coolDown;

                // 停止移动
                archerMovement.enabled = false;
                archerMovement.archerRb.velocity = Vector2.zero;
            }
            else if (detectEnemy())
            {
                // 停止移动
                archerMovement.enabled = false;
                //Debug.Log("检测到敌人，停止移动");
            }
            else if (!detectEnemy()) 
            {
                // 保持在行走状态
                archerMovement.enabled = true;
                archerMan.enabled = false;
                //Debug.Log("未检测到敌人，继续移动");

                // 继续移动
                archerMovement.archerRb.velocity = Vector2.right * archerMovement.speed;
            }
        }
        else if (archerState == ArcherState.Shooting)
        {
            if (!detectEnemy())
            {
                // 如果没有检测到敌人，立即切换回 Walking 状态
                archerState = ArcherState.Walking;
                archerMovement.enabled = true;
                archerMan.enabled = false;

                // 恢复移动速度
                archerMovement.archerRb.velocity = Vector2.right * archerMovement.speed;
            }
            else if (timer <= 0)
            {
                // 如果冷却结束并且敌人在范围内，继续攻击
                archerMan.Attack();
                timer = coolDown; // 重置冷却时间
            }
        }
    }

    private bool detectEnemy()
    {
        // 发射一个射线来检测前方的敌人
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + offset, Vector2.right, detectionRange, enemyLayer);
        Debug.DrawRay(transform.position + offset, Vector2.right * detectionRange, Color.red);

        foreach (RaycastHit2D hit in hits) // 遍历所有检测到的对象
        {
            if (hit.collider != null) // 确保命中了对象
            {
                //Debug.Log("Raycast Hit: " + hit.collider.gameObject.name + " | Tag: " + hit.collider.tag);


                    return true; // 只要有一个活着的敌人，就返回 true

            }
        }
        return false; // 如果没有检测到活着的敌人，返回 false
    }
}



// enumerations, creates a list of variables that we can choose from
public enum ArcherState
{
    Idle,
    Walking,
    Shooting
};