using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Defender : MonoBehaviour
{
    public string defenderName; // Defender 名称
    public int baseAttackPower; // 基础攻击力
    public float baseHealth;    // 基础生命值
    public int level = 1;       // 当前等级
    public int maxLevel = 25;   // 最大等级
    public int spawnCoolDown = 2;

    [Header("Upgrade Settings")]
    public int attackPowerPerLevel = 2;  // 每级提升的攻击力
    public float healthPerLevel = 5f;    // 每级提升的生命值
    public int initialUpgradeCost = 100; // 初始升级费用
    public float upgradeRate = 1.2f;     // 升级费用递增率

    public int attackPower;
    public float health;

    public DefenderHealth defenderHealth;
    private InventoryManager inventoryManager;

    [Header("Weapons")]
    public GameObject[] weapons; // 用于管理不同等级的武器

    [Header("Healing Settings")]
    public bool canHeal = false;   // 该 Defender 是否能进行治疗
    public int healThreshold = 5;  // 每多少次攻击触发一次治疗
    public int attackCounter = 0;  // 记录攻击次数
    public float healMultiplier = 2f; // 治疗系数（默认2倍攻击力）
    // **治疗量 = 计算后的攻击力 * 2** 随等级变化
    public float HealAmount => attackPower * healMultiplier;
    // 当治疗触发时通知 ArcherMan**
    public event Action OnHealTriggered;
    public float healDealy = 1.1f; //治疗delay
    public float healCompleteDealy = 1.1f; // 治疗后的Dealy, 时间结束后才能攻击
    public bool isHealing = false;

    [Header("Buff Settings")]
    public bool canBuff = false; // **是否能释放 Buff**
    public int buffThreshold = 3; // **每 3 次攻击触发**
    public float buffDuration = 5f; // **Buff 持续时间**
    public LayerMask allyLayer; // **友军的 Layer**
    public float buffRange = 5f; // **Buff 影响范围**
    public int attackBuffCounter = 0;
    // 当治疗触发时通知 ArcherMan**
    public event Action OnBuffTriggered;
    public bool isBuffing = false;
    public bool isBuffed = false; // **Track whether the Defender is already buffed**

    [Header("Area Healing Settings")]
    public bool canAreaHeal = false; // 是否可以进行群体治疗
    public int areaHealThreshold = 1;  // 每 3 次攻击触发一次治疗
    public float healRadius = 25f;  // 群体治疗的影响范围
    public int areaHealAttackCounter = 0;
    public float areaHealMultiplier = 5f; // 群体治疗系数
    public event Action OnAreaHealTriggered;

    // **动态计算升级花费**
    public int UpgradeCost => Mathf.CeilToInt(initialUpgradeCost * Mathf.Pow(upgradeRate, level - 1));

    private DefenderAudio defenderAudio;

    [Header("Equipped Items")]
    public Dictionary<EquipmentType, Equipment> equippedItems = new Dictionary<EquipmentType, Equipment>();


    private void Awake()
    {
        defenderAudio = GetComponent<DefenderAudio>();
    }

    private void Start()
    {
        level = DefenderDataManager.Instance.GetDefenderLevel(defenderName);
        inventoryManager = FindObjectOfType<InventoryManager>();

        // **使用 DefenderDataManager 加载装备**
        DefenderDataManager.Instance.RestoreDefenderEquipment(this);

        RecalculateState();
        ActivateWeaponBasedOnLevel(); // 统一武器激活逻辑

        //defenderAudio = GetComponent<DefenderAudio>();
    }

    // **初始化 Defender**
    public virtual void Initialize(DefenderBaseData baseData, int savedLevel)
    {
        if (baseData == null)
        {
            Debug.LogError("Base data is null. Cannot initialize Defender.");
            return;
        }

        defenderName = baseData.name;
        maxLevel = baseData.maxLevel > 0 ? baseData.maxLevel : 25;
        level = Mathf.Clamp(savedLevel, 1, maxLevel);

        RecalculateState();
        ActivateWeaponBasedOnLevel(); // 统一武器激活逻辑
    }

    // **装备物品**
    public void EquipItem(Equipment equipment)
    {
        equippedItems[equipment.type] = equipment;
        RecalculateState();
        //DefenderDataManager.Instance.SaveDefenderEquipment(defenderName, equippedItems);
    }

    // **卸下装备**
    public void UnequipItem(EquipmentType type)
    {
        if (equippedItems.ContainsKey(type))
        {
            equippedItems.Remove(type);
            RecalculateState();
            //DefenderDataManager.Instance.SaveDefenderEquipment(defenderName, equippedItems);
        }
    }

    // **重新计算属性**
    public void RecalculateState()
    {
        // **计算基础攻击力和生命值**
        int baseAttack = baseAttackPower + (level - 1) * attackPowerPerLevel;
        float baseHp = baseHealth + (level - 1) * healthPerLevel;

        // **获取当前装备的加成**
        int flatAttackBonus = 0;
        float percentAttackBonus = 0f;
        int flatHealthBonus = 0;
        float percentHealthBonus = 0f;

        // **改为使用当前 Defender 的装备，而不是全局装备列表**
        if (equippedItems.TryGetValue(EquipmentType.Weapon, out Equipment weapon))
        {
            flatAttackBonus += weapon.flatAttackBonus;
            percentAttackBonus += weapon.percentAttackBonus;
        }

        if (equippedItems.TryGetValue(EquipmentType.Ring, out Equipment ring))
        {
            flatAttackBonus += ring.flatAttackBonus;
            percentAttackBonus += ring.percentAttackBonus;
            flatHealthBonus += ring.flatHealthBonus;
            percentHealthBonus += ring.percentHealthBonus;
        }
        if (equippedItems.TryGetValue(EquipmentType.Armor, out Equipment armor))
        {
            flatHealthBonus += armor.flatHealthBonus;
            percentHealthBonus += armor.percentHealthBonus;
        }
        if (equippedItems.TryGetValue(EquipmentType.Boots, out Equipment boots))
        {
            flatHealthBonus += boots.flatHealthBonus;
            percentHealthBonus += boots.percentHealthBonus;
        }
        if (equippedItems.TryGetValue(EquipmentType.Helmet, out Equipment helmet))
        {
            flatHealthBonus += helmet.flatHealthBonus;
            percentHealthBonus += helmet.percentHealthBonus;
        }

        // **计算最终攻击力**
        attackPower = baseAttack + flatAttackBonus;
        attackPower = Mathf.RoundToInt(attackPower * (1 + percentAttackBonus));

        // **计算最终生命值**
        health = baseHp + flatHealthBonus;
        health *= (1 + percentHealthBonus);

        //Debug.Log($"[DEBUG] {defenderName} 计算后属性: 攻击力 = {attackPower}, 血量 = {health}");

        defenderHealth = GetComponent<DefenderHealth>();
        if (defenderHealth != null)
        {
            defenderHealth.maxHealth = health;
            defenderHealth.health = health;
            if (defenderHealth.sliderInstance != null)
            {
                defenderHealth.sliderInstance.value = health;
                defenderHealth.sliderInstance.maxValue = health;
            }
        }

    }



    // **升级逻辑**
    public virtual bool LevelUp()
    {
        inventoryManager = FindObjectOfType<InventoryManager>(); // 获取金币管理器

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager reference is null in LevelUp()");
            return false;
        }

        int currentCoins = inventoryManager.GetCurrencyAmount("Coin");
        //Debug.Log($"[DEBUG] {defenderName} 当前金币: {currentCoins}, 升级费用: {UpgradeCost}");


        if (level < maxLevel)
        {
            if (inventoryManager != null && inventoryManager.GetCurrencyAmount("Coin") >= UpgradeCost)
            {
                inventoryManager.AddItem(-UpgradeCost, "Coin");
                level++;
                RecalculateState();
                ActivateWeaponBasedOnLevel(); // 统一武器激活逻辑

                DefenderDataManager.Instance.SaveDefenderLevel(defenderName, level);

                Debug.Log($"{gameObject.name} 升级到 {level} 级！攻击力：{attackPower} 血量：{health}");
                return true;
            }
            else
            {
                Debug.LogWarning("Not enough coins.");
                return false;
            }
        }
        else
        {
            Debug.Log(gameObject.name + " 已达到最大等级！");
            return false;
        }
    }

    // **根据等级激活对应的武器**
    private void ActivateWeaponBasedOnLevel()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError($"[ERROR] {gameObject.name} 的 weapons[] 数组为空！");
            return;
        }

        int weaponIndex = Mathf.Clamp((level - 1) / 5, 0, weapons.Length - 1);

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == weaponIndex);
        }

        //Debug.Log($"[DEBUG] {gameObject.name} 当前等级: {level}, 激活武器索引: {weaponIndex}");
    }

    // **治疗自己**
    public void StartHealingCoroutine()
    {
        StartCoroutine(HealingSequence());
    }

    private IEnumerator HealingSequence()
    {
        if (defenderHealth != null)
        {
            Debug.Log($"[HEAL] {gameObject.name} starting healing animation...");

            // **触发治疗事件，通知 `ArcherMan` 播放治疗动画**
            OnHealTriggered?.Invoke();

            // **立即播放治疗音效**
            if (defenderAudio != null)
            {
                Debug.Log("[HEAL] Playing heal sound in HealingSequence()...");
                defenderAudio.PlayHealSound();
            }

            // **等待 1.1 秒后执行治疗**
            yield return new WaitForSeconds(healDealy);

            


            float healValue = HealAmount;
            defenderHealth.health = Mathf.Min(defenderHealth.health + healValue, defenderHealth.maxHealth);
            if (defenderHealth.sliderInstance != null)
            {
                defenderHealth.sliderInstance.value = defenderHealth.health;
            }
            Debug.Log($"[HEAL] {gameObject.name} healed for {healValue} (Attack Power: {attackPower} * Multiplier: {healMultiplier}). Current HP: {defenderHealth.health}");
 
            // **等待治疗动画完全播放完（额外 0.9 秒）**
            yield return new WaitForSeconds(healCompleteDealy);

            Debug.Log($"[HEAL] {gameObject.name} healing animation completed.");

            // **恢复攻击能力**
            isHealing = false;
        }
    }

    // **攻击计数，用于触发治疗**
    public void RegisterAttack()
    {
        if (!canHeal || isHealing) return; // 只有可治疗的 Defender 才计算攻击次数

        attackCounter++;
        if (attackCounter >= healThreshold)
        {
            isHealing = true; // **立刻标记正在治疗**
            StartHealingCoroutine();
            attackCounter = 0; // 触发治疗后重置计数
        }
    }

    // **重置生命值**
    public void Reset()
    {
        health = baseHealth;
        if (defenderHealth != null)
        {
            defenderHealth.health = health;
            defenderHealth.sliderInstance.value = health;
        }
    }

    // **攻击计数 & 触发 Buff**
    public void RegisterAttackBuff()
    {
        attackBuffCounter++;

        // **每 3 次攻击触发 Buff**
        if (canBuff && attackBuffCounter >= buffThreshold)
        {
            StartBuffCoroutine();
            attackBuffCounter = 0; // **重置计数**
        }
    }

    public void RegisterAttackAreaHeal()
    {
        areaHealAttackCounter++;

        // **每 3 次攻击触发 AreaHeal**
        if (canAreaHeal && areaHealAttackCounter >= areaHealThreshold)
        {
            StartCoroutine(ApplyAreaHealing());
            areaHealAttackCounter = 0; // **重置计数**
        }
    }

    public void StartBuffCoroutine()
    {
        StartCoroutine(ApplyBuffToRandomDefender());
    }

    // **给随机队友加 Buff**
    private IEnumerator ApplyBuffToRandomDefender()
    {
        isBuffing = true;

        // **查找范围内的所有友军**
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, buffRange, allyLayer);
        if (allies.Length == 0)
        {
            isBuffing = false;
            yield break;
        }

        // **过滤掉已经被 Buff 的 Defender**
        List<Defender> availableDefenders = new List<Defender>();
        foreach (Collider2D ally in allies)
        {
            Defender defender = ally.GetComponent<Defender>();
            if (defender != null && !defender.isBuffed && defender.name != "Elysia(Clone)") // **只选择未 Buff 过的**
            {
                availableDefenders.Add(defender);
            }
        }

        // **如果没有合适的目标，终止 Buff**
        if (availableDefenders.Count == 0)
        {
            Debug.Log("[BUFF] No available defenders to buff!");
            isBuffing = false;
            yield break;
        }

        // **从可选目标中随机选择一个**
        Defender target = availableDefenders[UnityEngine.Random.Range(0, availableDefenders.Count)];

        // **应用 Buff**
        if (target != null)
        {
            StartCoroutine(ApplyBuff(target));
        }
        else
        {
            isBuffing = false; // **确保 Buff 状态重置**
        }
    }

    // **Buff 逻辑**
    private IEnumerator ApplyBuff(Defender target)
    {
        // **Check if Defender is already buffed**
        if (target.isBuffed)
        {
            Debug.Log($"[BUFF] {target.gameObject.name} is already buffed! Skipping...");
            yield break; // **Exit without applying Buff again**
        }

        // **Mark Defender as buffed**
        target.isBuffed = true;

        // **等待 动画 持续时间**
        yield return new WaitForSeconds(2f);

        Debug.Log($"[BUFF] {gameObject.name} is buffing {target.gameObject.name}!");

        // **触发治疗事件，通知 `ArcherMan` 播放治疗动画**
        OnBuffTriggered?.Invoke();

        // **确保 AudioSource 正确获取**
        //if (defenderAudio == null)
        //{
        //    defenderAudio = GetComponent<DefenderAudio>();
        //}

        // **播放 Buff 音效**
        if (defenderAudio != null && defenderAudio.spellSound != null)
        {
            Debug.Log("[BUFF] Playing Buff sound...");
            defenderAudio.PlaySpellSound();
        }
        else
        {
            Debug.LogWarning("[BUFF] Buff sound is missing or defenderAudio is null!");
        }

        // **保存原始大小和颜色**
        Vector3 originalScale = target.transform.localScale;

        Vector3 buffedScale = originalScale * 1.2f;


        // **获取 Defender 上所有的 SpriteRenderer**
        SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>();

        // **保存原始颜色，并全部变红**
        Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            originalColors[sr] = sr.color;
            //sr.color = Color.red; // **变红**
        }

        // **缓慢放大 Defender 并变红**
        float duration = 0.5f; // 缩放 & 变色持续时间
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // **缩放**
            target.transform.localScale = Vector3.Lerp(originalScale, buffedScale, t);

            // **颜色渐变**
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                sr.color = Color.Lerp(originalColors[sr], Color.red, t);
            }

            yield return null;
        }

        // **确保最终颜色 & 大小完全变化**
        target.transform.localScale = buffedScale;
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.color = Color.red;
        }

        // **保存原始 Mass 和 Linear Drag**
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        float originalMass = rb != null ? rb.mass : 0;
        float originalDrag = rb != null ? rb.drag : 0;

        if (rb != null)
        {
            rb.mass = 50f;
            rb.drag = 50f;
            Debug.Log($"[BUFF] {target.gameObject.name} Mass & Drag increased to 50.");
        }

        // **提升攻击力**
        int originalAttackPower = target.attackPower;
        target.attackPower += attackPower;
        Debug.Log($"[BUFF] {target.gameObject.name} attackPower: {originalAttackPower} → {target.attackPower}");

        // **减少攻击冷却，并修改 detectionRange**
        ArcherStatementMachine archMachine = target.GetComponent<ArcherStatementMachine>();
        float originalCooldown = -1f;
        float originalDetectionRange = 0f;
        if (archMachine != null)
        {
            originalCooldown = archMachine.coolDown;
            originalDetectionRange = archMachine.detectionRange;
            archMachine.coolDown *= 0.7f; // **减少 30%**
            archMachine.detectionRange *= 1.3f; // **Expand detection range by 20%**
            Debug.Log($"[BUFF] {target.gameObject.name} attack cooldown: {originalCooldown} → {archMachine.coolDown}");       
        }

        // **等待 Buff 持续时间**
        yield return new WaitForSeconds(buffDuration);

        // **恢复原攻击力**
        target.attackPower = originalAttackPower;

        // **恢复 coolDown**
        if (archMachine != null && originalCooldown > 0)
        {
            archMachine.coolDown = originalCooldown;
            archMachine.detectionRange = originalDetectionRange;
        }

        // **缓慢缩小 Defender 并恢复颜色**
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // **缩放**
            target.transform.localScale = Vector3.Lerp(buffedScale, originalScale, t);

            // **颜色恢复**
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                sr.color = Color.Lerp(Color.red, originalColors[sr], t);
            }

            yield return null;
        }

        // **确保最终恢复**
        target.transform.localScale = originalScale;
        foreach (var entry in originalColors)
        {
            entry.Key.color = entry.Value;
        }

        // **恢复 Mass 和 Linear Drag**
        if (rb != null)
        {
            rb.mass = originalMass;
            rb.drag = originalDrag;
            Debug.Log($"[BUFF] {target.gameObject.name} Mass & Drag restored to original values.");
        }

        // **Mark Defender as no longer buffed**
        target.isBuffed = false;

        Debug.Log($"[BUFF] {target.gameObject.name} Buff expired, restoring original stats.");
    }


    // **群体治疗 Coroutine**
    private IEnumerator ApplyAreaHealing()
    {
        Debug.Log($"[HEAL] {gameObject.name} is casting area healing...");

        yield return new WaitForSeconds(1f); // **延迟animation**

        // **触发治疗动画**
        OnAreaHealTriggered?.Invoke();

        // **播放治疗音效**
        if (defenderAudio != null && defenderAudio.healSound != null)
        {
            Debug.Log("[Heal] Playing Heal sound...");
            defenderAudio.PlayHealSound();
        }
        else
        {
            Debug.LogWarning("[Heal] Heal sound is missing or defenderAudio is null!");
        }

        yield return new WaitForSeconds(1f); // **延迟治疗**
        // **获取范围内所有友军**
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, healRadius, allyLayer);

        if (allies.Length == 0)
        {
            Debug.Log("[HEAL] No allies in range for area healing!");
            yield break;
        }

        // **遍历所有友军，执行治疗**
        foreach (Collider2D ally in allies)
        {
            Defender allyDefender = ally.GetComponent<Defender>();
            if (allyDefender != null && allyDefender.defenderHealth != null)
            {
                float healValue = attackPower * healMultiplier;
                allyDefender.defenderHealth.health = Mathf.Min(
                    allyDefender.defenderHealth.health + healValue,
                    allyDefender.defenderHealth.maxHealth
                );

                if (allyDefender.defenderHealth.sliderInstance != null)
                {
                    allyDefender.defenderHealth.sliderInstance.value = allyDefender.defenderHealth.health;
                }

                // **显示治疗数值**
                allyDefender.defenderHealth.ShowDamage((int)healValue, true);

                //Debug.Log($"[HEAL] {allyDefender.gameObject.name} healed for {healValue}, new HP: {allyDefender.defenderHealth.health}");
            }
        }

        yield return new WaitForSeconds(0.5f); // **缓冲时间**
        Debug.Log($"[HEAL] {gameObject.name} area healing completed.");
    }

    // **可视化治疗半径**
    void OnDrawGizmosSelected()
    {
        if (healRadius > 0) // 确保半径有效
        {
            Gizmos.color = Color.green; // 设置颜色为绿色
            Gizmos.DrawWireSphere(transform.position, healRadius); // 画圆形范围
        }
    }

}
