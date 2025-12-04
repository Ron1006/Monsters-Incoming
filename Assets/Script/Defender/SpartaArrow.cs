using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpartaArrow : MonoBehaviour
{
    public Rigidbody2D arrowRb;
    public float speed = 5f;
    public float range = 0.1f;
    private float timer;
    public int damage;
    public float knockbackForce;
    private bool hasHit = false; // 用于标记是否已命中 

    //public RangeBowMan bowMan;

    // Start is called before the first frame update
    void Start()
    {
        timer = range;
    }

    // Update is called 50 times per frame
    void FixedUpdate()
    {


        if (!hasHit) //只在没有命中时才继续移动
        {
            arrowRb.velocity = Vector2.right * speed;
        }

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return; // 如果已经命中，不再处理新的碰撞

        if (collision.gameObject.GetComponent<EnemyHealth>())
        {
            // 标记为已命中
            hasHit = true;

            // 处理敌人击退和伤害
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(Vector2.right * knockbackForce, ForceMode2D.Impulse);
            }

            collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);

            Destroy(gameObject);
        }

        else if (collision.gameObject.GetComponent<CaveHealth>())
        {
            // 标记为已命中
            hasHit = true;

            // 对洞穴造成伤害
            collision.gameObject.GetComponent<CaveHealth>().TakeDamage(damage);
            Destroy(gameObject); // 销毁箭矢
        }
    }

    public void SetDamage(int attackPower)
    {
        damage = attackPower;
        Debug.Log("Arrow damage set to: " + damage);
    }
}
