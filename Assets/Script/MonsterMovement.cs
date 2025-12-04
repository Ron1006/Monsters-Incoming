using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    public Rigidbody2D monsterRb;
    public float speed;
    private bool canMove = true; // 控制是否可以移动
    public int defenderLayer = 7; // 设置为指定的 Layer 索引，例如 8 代表 Defender 层

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called 50 times per second
    void FixedUpdate()
    {
        // 如果可以移动，则向左移动
        if (canMove)
        {
            monsterRb.velocity = Vector2.left * speed;
        }
        else
        {
            // 停止移动
            monsterRb.velocity = Vector2.zero;
        }
    }


    // 当怪物与其他物体发生碰撞时触发
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果碰到的物体在指定的 Layer（例如 Defender 层），则停止移动
        if (collision.gameObject.layer == defenderLayer)
        {
            StopMovementTemporarily();
        }
    }

    // 停止移动一段时间，之后恢复
    void StopMovementTemporarily()
    {
        // 停止移动
        canMove = false;

        // 在 1 秒后恢复移动
        Invoke("ResumeMovement", 1f); 
    }

    // 恢复移动
    void ResumeMovement()
    {
        canMove = true;
    }

    public void EnableMovement(bool enable)
    {
        canMove = enable;
    }
}
