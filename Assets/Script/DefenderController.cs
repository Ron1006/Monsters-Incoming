using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderController : MonoBehaviour
{
    public Animator animator;
    public string attackTriggerName = "Attack"; // 你可以用这个来控制触发器名称

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // 触发攻击动画
    public void TriggerAttack()
    {
        animator.SetTrigger(attackTriggerName);  // 触发 Attack 参数
    }


}
