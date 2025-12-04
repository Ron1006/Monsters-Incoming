using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherMan : MonoBehaviour
{
    public GameObject arrowPrefab;
    //public float coolDown = 2.5f;
    //public float timer;
    private DefenderAudio defenderAudio;
    public Transform arrowSpawnPoint;
    public Rigidbody2D archerRb;

    private Animator weaponAnimator; // 引用 Animator 组件
    public string attackTriggerName = "Attack";
    public string healTriggerName = "Heal"; // 治疗动画的触发器**
    public string buffTriggerName = "Buff"; // 治疗动画的触发器**
    public string areaHealTriggerName = "AreaHeal"; // 治疗动画的触发器**
    public float arrowSpawnDealy = 0.5f; // 箭矢延迟发射时间
    public Defender defender; // 引用 Defender 类

    // Start is called before the first frame update
    void Start()
    {
        //timer = 0;
        
        // 延迟 0.1 秒调用 SetActiveWeaponAnimator 以确保 Defender 已完全初始化
        Invoke("SetActiveWeaponAnimator", 0.1f);

        // **监听 Defender 的 `OnHealTriggered` 事件**
        if (defender != null)
        {
            defender.OnHealTriggered += PlayHealAnimation;
            defender.OnBuffTriggered += PlayBuffAnimation;
            defender.OnAreaHealTriggered += PlayAreaHealAnimation;
        }

        defenderAudio = GetComponent<DefenderAudio>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 查找激活的武器的animator
    void SetActiveWeaponAnimator()
    {
        // 重置 Animator
        weaponAnimator = null;
        //Debug.Log($"[DEBUG] Setting active weapon animator for: {gameObject.name}");

        // 遍历所有武器子对象
        for (int i = 1; i <= 5; i++)
        {
            Transform weaponTransform = transform.Find($"Weapon_lv{i}");

            if (weaponTransform == null)
            {
                //Debug.LogWarning($"[ERROR] {gameObject.name}: Cannot find Weapon_lv{i}");
                continue;
            }
            //Debug.Log($"[INFO] Found {weaponTransform.name}, Active: {weaponTransform.gameObject.activeSelf}");

            if (!weaponTransform.gameObject.activeSelf)
            {
                //Debug.Log($"[INFO] {weaponTransform.name} is inactive, skipping...");
                continue;
            }

            // 在这里检查是否能获取 Animator
                // 查找第一级子对象以及其子目录中的 Animator
                weaponAnimator = weaponTransform.GetComponent<Animator>();
                if (weaponAnimator == null)
                {
                    //Debug.LogWarning($"[ERROR] {weaponTransform.name} does not have an Animator component!");
                }
                else
                {
                    //Debug.Log($"[SUCCESS] Found Animator on {weaponTransform.name}");
                    break;
                }

        }

        // 如果没有找到激活的武器或 Animator 组件，输出错误信息
        if (weaponAnimator == null)
        {
            Debug.LogError($"[FATAL] {gameObject.name}: No active weapon with an Animator found!");
        }
    }

    public void Attack()
    {
        if (defender != null && defender.isHealing)
        {
            Debug.Log("[HEAL] Defender is healing, attack canceled.");
            return; // **正在治疗时，阻止攻击**
        }

        if (weaponAnimator == null)
        {
            Debug.LogWarning("WeaponAnimator is not initialized. Attempting to set it now.");
            SetActiveWeaponAnimator(); // 如果未初始化，尝试重新设置
        }

        if (weaponAnimator != null)
        {
            StartCoroutine(SpawnArrowWithDelay());

            // 让 Defender 记录攻击次数，并决定是否触发治疗
            if (defender != null)
            {
                defender.RegisterAttack();
            }
            if (defender != null)
            {
                defender.RegisterAttackBuff();
            }
            if (defender != null)
            {
                defender.RegisterAttackAreaHeal();
            }
        }
        else
        {
            Debug.Log("WeaponAnimator is still null. Attack cannot proceed.");
        }
    }
    private IEnumerator SpawnArrowWithDelay()
    {
        // **如果 Defender 处于治疗状态，则等待治疗完成**
        if (defender != null && defender.isHealing)
        {
            Debug.Log("[HEAL] Waiting for healing animation to complete...");
            while (defender.isHealing)
            {
                yield return null; // 等待下一帧再检查
            }
        }

        weaponAnimator.SetTrigger(attackTriggerName);
        yield return new WaitForSeconds(arrowSpawnDealy);

        

        // **判断是否触发治疗**
        if (defender != null && defender.canHeal && defender.attackCounter >= defender.healThreshold)
        {

            Debug.Log("[HEAL] Direct healing triggered instead of spawning an arrow.");
            defender.isHealing = true;  // **标记正在治疗**
            defender.StartHealingCoroutine(); // **直接调用 Heal()，不生成箭矢**
            defenderAudio.PlayHealSound();
            defender.attackCounter = 0; // **重置攻击计数**
        }

        // **判断是否触发 Buff**
        else if (defender != null && defender.canBuff && defender.attackCounter >= defender.buffThreshold)
        {
            Debug.Log("[BUFF] Buff triggered instead of spawning an arrow.");
            defender.isBuffing = true;  // **标记正在 Buff**
            defender.StartBuffCoroutine(); // **直接调用 Buff 逻辑**
            defender.attackCounter = 0; // **重置攻击计数**
        }

        else
        {
            if (defenderAudio != null)
            {
                defenderAudio.PlayAttackSound();
            }
            // 生成箭矢
            GameObject arrowInstance = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

            // 获取 Arrow 脚本并传递攻击力
            Arrow arrowScript = arrowInstance.GetComponent<Arrow>();
            if (arrowScript != null && defender != null)
            {
                arrowScript.SetDamage(defender.attackPower);
            }
            else
            {
                Debug.LogWarning("Arrow script or Defender not found.");
            }
            //获取 IceCloud 脚本并传递攻击力
            IceCloud cloudScript = arrowInstance.GetComponent<IceCloud>();
            if(cloudScript != null)
            {
                cloudScript.damage = defender.attackPower;
            }
        }


    }

    // **监听 `Defender` 触发的治疗事件，并播放治疗动画**
    private void PlayHealAnimation()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger(healTriggerName);
            Debug.Log($"[ANIMATION] {gameObject.name} played Heal animation.");
        }
    }

    // **监听 `Defender` 触发的Buff事件，并播放Buff动画**
    private void PlayBuffAnimation()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger(buffTriggerName);
            Debug.Log($"[ANIMATION] {gameObject.name} played Buff animation.");
        }
    }

    // **监听 `Defender` 触发的AreaHeal事件，并播放AreaHeal动画**
    private void PlayAreaHealAnimation()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger(areaHealTriggerName);
            Debug.Log($"[ANIMATION] {gameObject.name} played areaHeal animation.");
        }
    }
}
