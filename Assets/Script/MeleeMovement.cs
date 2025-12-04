using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeMovement : MonoBehaviour
{
    public Rigidbody2D meleeRb;
    public float speed;
    private bool canMove = true; // 控制是否可以移动
    public int monsterLayer = 6; // 设置为指定的 Layer 索引，例如 7 代表 Defender 层
    public float stopMoveTime = 1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called 50 times per second
    void FixedUpdate()
    {
        // 如果可以移动，则向右移动
        if (canMove)
        {
            meleeRb.velocity = Vector2.right * speed;
        }
        else
        {
            // 停止移动
            meleeRb.velocity = Vector2.zero;
        }
    }

    // 当怪物与其他物体发生碰撞时触发
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果碰到的物体在指定的 Layer（例如 Defender 层），则停止移动
        if (collision.gameObject.layer == monsterLayer)
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
        Invoke("ResumeMovement", stopMoveTime);
    }

    // 恢复移动
    void ResumeMovement()
    {
        canMove = true;
    }
}
