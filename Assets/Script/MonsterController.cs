using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public Animator animator;
    public string attackTriggerName = "Attack"; // 你可以用这个来控制触发器名称
    public string damagedTriggerName = "Damaged"; // 控制受伤的触发器名称

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 检查动画参数是否存在
    private bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    // 触发攻击动画
    public void TriggerAttack()
    {
        animator.SetTrigger(attackTriggerName);  // 触发 Attack 参数
    }

    // 触发受伤动画
    public void TriggerDamaged()
    {
        if (animator != null && HasParameter(damagedTriggerName))
        {
            animator.SetTrigger(damagedTriggerName);  // 触发 Damaged 参数
        }
        
    }
}
