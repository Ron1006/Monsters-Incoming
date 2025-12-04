using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Arrow : MonoBehaviour
{
    public Rigidbody2D arrowRb;
    public float speed = 5f;
    public float range;
    private float timer;
    public int damage;
    public float knockbackForce;
    private bool hasHit = false; // 用于标记是否已命中 

    private AudioSource audioSource;
    public AudioClip explosionSound;
    private Animator animator;

    [Header("Splash Setting")]
    public float splashRange = 2f; //溅射范围
    public float splashDamagePercentage = 0.2f; //溅射伤害占主伤害比例
    public bool enableSplashDamage = false;

    //public RangeBowMan bowMan;

    // Start is called before the first frame update
    void Start()
    {
        timer = range;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        //Debug.Log($"Splash Range: {splashRange}, Splash Damage Percentage: {splashDamagePercentage}");
    }

    // Update is called 50 times per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (!hasHit) //只在没有命中时才继续移动
        {
            arrowRb.velocity = Vector2.right * speed;
        }



        
        if (timer <= 0)
        {
            arrowRb.velocity = Vector2.zero; // 停止移动
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

            //PlayExplosionSound();
            // 处理敌人击退和伤害
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(Vector2.right * knockbackForce, ForceMode2D.Impulse);
            }

            collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);
            if (enableSplashDamage)
            {
                TriggerExplosion();
            }
            else
            {
                Destroy(gameObject);
            }
            
            //Destroy(gameObject);
        }

        else if (collision.gameObject.GetComponent<CaveHealth>())
        {
            // 标记为已命中
            hasHit = true;

            //PlayExplosionSound();
            // 对洞穴造成伤害
            collision.gameObject.GetComponent<CaveHealth>().TakeDamage(damage);
            if (enableSplashDamage)
            {
                TriggerExplosion();
            }
            else
            {
                Destroy(gameObject);
            }
            //Destroy(gameObject); // 销毁箭矢
        }
    }

    public void SetDamage(int attackPower)
    {
        damage = attackPower;
        //Debug.Log("Arrow damage set to: " + damage);
    }

    private void TriggerExplosion()
    {
        // 停止移动
        speed = 0;

        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        PlayExplosionSound();
        ApplySplashDamage();

    }

    private void ApplySplashDamage()
    {
        if (!enableSplashDamage) return;

        // 获取范围内的所有碰撞体
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, splashRange);
        float splashDamage = damage * splashDamagePercentage;

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.gameObject == gameObject) continue;

            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            CaveHealth caveHealth = collider.GetComponent<CaveHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)splashDamage);
            }
            else if (caveHealth != null)
            {
                caveHealth.health -= (int)splashDamage;
            }
        }
    }

    public void DestroyBullet()
    {
        Debug.Log("Explosion animation finished. Destroying bullet.");
        Destroy(gameObject);
    }

    private void PlayExplosionSound()
    {
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or ExplosionSound is not set!");
        }
    }
}
