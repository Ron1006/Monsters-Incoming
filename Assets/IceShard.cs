using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceShard : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 5;
    public float maxLifetime = 3f;

    private float lifetime;
    private Vector3 moveDirection = Vector3.down; // 默认向下

    void Start()
    {
        lifetime = maxLifetime;
    }

    void Update()
    {
        transform.Translate(moveDirection.normalized * speed * Time.deltaTime);

        lifetime -= Time.deltaTime;
        if(lifetime < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }

        // 如果碰到的是 Ground 层，也销毁
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("Hit the ground, destroying IceShard.");
            Destroy(gameObject);
        }

        CaveHealth cave = collision.gameObject.GetComponent<CaveHealth>();
        if (cave != null)
        {
            cave.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    public void SetDamage(int dmg) { damage = dmg; }

    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir;

    }
}
