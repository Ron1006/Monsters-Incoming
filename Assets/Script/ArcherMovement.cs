using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherMovement : MonoBehaviour
{
    public float speed;
    public Rigidbody2D archerRb;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called 50x per frame
    void FixedUpdate()
    {
        if (GetComponent<ArcherStatementMachine>().archerState == ArcherState.Walking)
        {
            archerRb.velocity = Vector2.right * speed;
        }
        else
        {
            archerRb.velocity = Vector2.zero;  // **防止 `Update()` 设定的 `velocity` 被 FixedUpdate() 覆盖**
        }
    }
}
