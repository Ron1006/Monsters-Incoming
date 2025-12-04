using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MoveManWithButtons : MonoBehaviour
{
    public GameObject man;
    public float offsetY = 0.3f; // 小人高度偏移
    public List<Transform> levelPositions; // 所有level按钮的位置列表 (按顺序添加)
    private bool isMoving = false;
    public float moveSpeed = 0.1f;
    private Vector3 targetPosition; // 目标位置

    private int currentIndex = 0; // 当前位置索引

    // Start is called before the first frame update
    void Start()
    {
        if (levelPositions.Count > 0)
        {
            SetManPosition(levelPositions[currentIndex]);
        }
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

    // 前进到下一个按钮
    public void MoveForward()
    {
        if (currentIndex < levelPositions.Count - 1)
        {
            currentIndex++;
            MoveToTarget(levelPositions[currentIndex]);
        }
    }

    public void MoveBackward()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            MoveToTarget(levelPositions[currentIndex]);
        }
    }

    // 移动到指定目标位置
    private void MoveToTarget(Transform target)
    {
        if (target != null)
        {
            targetPosition = new Vector3(target.position.x, target.position.y + offsetY, -1f);
            isMoving = true; // 设置移动标志
        }
    }

    // 设置小人位置
    private void SetManPosition(Transform targetPosition)
    {
        if (targetPosition != null)
        {
            man.transform.position = new Vector3(targetPosition.position.x, targetPosition.position.y + offsetY, -1f);
        }
    }

    // 获取当前level名称
    public string GetCurrentLevelName()
    {
        if (currentIndex >= 0 && currentIndex < levelPositions.Count)
        {
            return levelPositions[currentIndex].name;
        }
        return "Level1"; // 默认返回Level1
    }

    //点位点击触发
    public void MoveToLevelByClick(Transform targetLevel)
    {
        int targetIndex = levelPositions.IndexOf(targetLevel);

        if (targetIndex == -1)
        {
            Debug.LogWarning("The target level is not in the list!");
            return;
        }

        StartCoroutine(MoveSequentially(targetIndex));
    }

    //顺序移动协程
    private IEnumerator MoveSequentially(int targetIndex)
    {
        while (currentIndex != targetIndex)
        {
            if (currentIndex < targetIndex)
            {
                MoveForward();
            }
            else if (currentIndex > targetIndex)
            {
                MoveBackward();
            }

            // 等待小人到达目标位置后再继续移动
            while (isMoving)
            {
                yield return null;
            }
        }
    }
}
