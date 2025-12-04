using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMonster : MonoBehaviour
{
    [Header("Bullet Setting")]
    public int damage = 10;
    public float speed = 5f;
    public float lifetime = 5f;
    public float knockbackForce = 50f;
    private Animator animator;
    private bool hasExploded = false; // 防止重复爆炸
    private AudioSource audioSource;
    public AudioClip explosionSound;

    [Header("Splash Setting")]
    public float splashRange = 2f; //溅射范围
    public float splashDamagePercentage = 0.3f; //溅射伤害占主伤害比例
    public bool enableSplashDamage = false;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!hasExploded)
        {
            // 炮弹持续向前飞行
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return; // 防止重复触发
        hasExploded = true;

        // 检测是否击中目标
        DefenderHealth defenderHealth = collision.gameObject.GetComponent<DefenderHealth>();
        MeleeHealth meleeHealth = collision.gameObject.GetComponent<MeleeHealth>();
        TowerHealth towerHealth = collision.gameObject.GetComponent<TowerHealth>();
        Rigidbody2D defenderRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (defenderHealth != null && defenderRb != null )
        {
            
            // 对目标造成伤害
            defenderHealth.TakeDamage(damage);



            //// 计算击退方向
            //Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;

            //// 应用击退力
            //defenderRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            TriggerExplosion();

        }

        else if (meleeHealth != null && meleeHealth != null)
        {

            // 对目标造成伤害
            meleeHealth.TakeDamage(damage);

            //// 计算击退方向
            //Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;

            //// 应用击退力
            //defenderRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            TriggerExplosion();
        }

        if (towerHealth != null)
        {
            Debug.Log($"[BULLET] Tower hit! Damage: {damage}");
            towerHealth.TakeDamage(damage); // **改为 TakeDamage()**
            TriggerExplosion();
        }
        else
        {
            //Debug.Log($"[BULLET] Collision detected, but TowerHealth is NULL on {collision.gameObject.name}");
        }
    }

    private void TriggerExplosion()
    {
        // 停止移动
        speed = 0;
        hasExploded = true; // 防止重复触发

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

            DefenderHealth defenderHealth = collider.GetComponent<DefenderHealth>();
            MeleeHealth meleeHealth = collider.GetComponent<MeleeHealth>();
            TowerHealth towerHealth = collider.GetComponent<TowerHealth>();

            if (defenderHealth != null)
            {
                defenderHealth.TakeDamage((int)splashDamage);
            }
            else if (meleeHealth != null)
            {
                meleeHealth.TakeDamage((int)splashDamage);
            }
            else if (towerHealth != null)
            {
                towerHealth.health -= (int)splashDamage;
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

    private void OnDrawGizmosSelected()
    {
        if (enableSplashDamage)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, splashRange);
        }
    }
}
