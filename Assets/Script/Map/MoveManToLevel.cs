using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManToLevel : MonoBehaviour
{
    public GameObject man;
    public float moveSpeed = 55f;
    private Vector3 targetPosition; // Target position for the man
    private bool isMoving = false;

    private Transform currentLevel;// 保存当前Level的位置

    public void MoveToLevel(Transform levelPosition)
    {
        // Set the target position (x, y from level, z = -1)
        targetPosition = new Vector3(levelPosition.position.x, levelPosition.position.y + 0.3f, -1f);
        isMoving = true;

        currentLevel = levelPosition;
    }

    private void Update()
    {
        if (isMoving)
        {
            // Move the man towards the target position smoothly
            man.transform.position = Vector3.MoveTowards(man.transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // If the man has reached the target position, stop moving
            if (Vector3.Distance(man.transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
            }
        }
    }

    //返回当前选中的Level名称
    public string GetCurrentLevelName()
    {
        if (currentLevel !=null)
        {
            return currentLevel.gameObject.name;     
        }
        return "Level1"; //没有选中任何level返回空值
    }
}
