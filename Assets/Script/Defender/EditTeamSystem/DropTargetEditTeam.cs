using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropTargetEditTeam : MonoBehaviour, IDropHandler
{
    public Transform containerTransform; // Edit Team Container 的 Transform
    public Transform[] dropTargetPanel;

    private void Start()
    {
        // 用一个临时列表存储要删除的对象
        List<Transform> defendersToRemove = new List<Transform>();

        // 遍历 editTeamContainer 中的所有子对象
        for (int i = containerTransform.childCount - 1; i >= 0; i--)
        {
            Transform defender = containerTransform.GetChild(i);
            string defenderName = defender.name;
            //Debug.Log("container " +  defenderName);

            // 如果在 dropTargetPanel 中找到同名 defender，则删除
            if (HasSameDefenderInPanel(dropTargetPanel, defenderName))
            {
                //Debug.Log($"Defender {defenderName} found in dropTargetPanel. Removing from editTeamContainer.");
                defendersToRemove.Add(defender);
            }
        }

        // 删除标记的对象
        foreach (Transform defender in defendersToRemove)
        {
            Destroy(defender.gameObject);
        }

        if (containerTransform == null)
        {
            containerTransform = transform; // 默认使用当前 Transform
        }

        Debug.Log("Checking default defenders in EditTeamContainer...");



        // **确保至少有 2 个面板**
        //if (dropTargetPanel.Length < 2)
        //{
        //    Debug.LogWarning("Not enough drop target panels assigned!");
        //    return;
        //}

        
    }

    private void DisableDefenderFromContainer(string defenderName)
    {
        string targetName = NormalizeName(defenderName);

        for (int i = containerTransform.childCount - 1; i >= 0; i--)
        {
            Transform defender = containerTransform.GetChild(i);
            string currentName = NormalizeName(defender.name);

            //Debug.Log($"[检查] 当前: {currentName} vs 目标: {targetName}");

            if (currentName == targetName)
            {
                //Debug.Log($"✅ 匹配成功，禁用: {defender.name}");
                defender.gameObject.SetActive(false);
                //Debug.Log($"禁用后对象状态: {defender.name}, activeSelf = {defender.gameObject.activeSelf}");
            }
        }
    }

    private void ActiveDefenderFromContainer(string defenderName)
    {
        // **检查 `dropTargetPanel` 和 `containerTransform` 里是否有该 Defender**
        if (!HasSameDefenderInPanel(dropTargetPanel, defenderName) && !HasSameDefenderInContainer(defenderName))
        {
            Debug.Log($"{defenderName} 不在队伍中，尝试启用备用 Defender...");

            // **遍历 `EditTeamContainer` 里的 Defender**
            for (int i = 0; i < containerTransform.childCount; i++)
            {
                Transform defender = containerTransform.GetChild(i);
                if (NormalizeName(defender.name) == NormalizeName(defenderName))
                {
                    // **启用 Defender**
                    defender.gameObject.SetActive(true);
                    Debug.Log($"{defender.name} 已被启用！");
                    return;
                }
            }

            Debug.LogWarning($"未找到 {defenderName}，无法启用！");
        }
        else
        {
            Debug.Log($"{defenderName} 已在队伍中，无需重新启用！");
        }
    }


    private void AddDefenderToPanel(GameObject defenderPrefab, Transform panel)
    {
        if (defenderPrefab == null)
        {
            Debug.LogError("Defender prefab is missing!");
            return;
        }

        if (panel == null)
        {
            Debug.LogError(" Panel is NULL!");
            return;
        }

        //Debug.Log($"Instantiating {defenderPrefab.name} in {panel.name}");

        // **实例化 Defender**
        GameObject newDefender = Instantiate(defenderPrefab, panel);
        newDefender.transform.localPosition = Vector3.zero;
        newDefender.transform.localScale = Vector3.one;
        newDefender.SetActive(true);

        // **让 Defender 进入 Panel**
        DraggableDefender draggableDefender = newDefender.GetComponent<DraggableDefender>();
        if (draggableDefender != null)
        {
            draggableDefender.OnDefenderRemoved?.Invoke(); // **触发 Defender 更新**
        }

        //Debug.Log($"成功添加 {newDefender.name} 到 {panel.name}");
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop triggered for Edit Team Panel.");

        if (eventData.pointerDrag == null)
        {
            Debug.Log("No object is being dragged.");
            return;
        }

        // 获取被拖拽的对象
        GameObject draggedObject = eventData.pointerDrag;

        // 检查拖拽对象是否有 DraggableDefender 组件
        DraggableDefender draggableDefender = draggedObject.GetComponent<DraggableDefender>();
        if (draggableDefender == null)
        {
            Debug.Log("Dragged object does not have a DraggableDefender component.");
            return;
        }


        // 将拖动对象设置为目标面板的子对象
        draggedObject.transform.SetParent(containerTransform);

        // 恢复拖拽对象的交互性
        CanvasGroup canvasGroup = draggedObject.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true; // 允许射线检测
        }
        Debug.Log($"Object successfully dropped onto {gameObject.name}.");
    }

    private bool HasSameDefenderInPanel(Transform[] panels, string defenderName)
    {
        string normalizedName = NormalizeName(defenderName);

        //Debug.Log($"[DEBUG] Looking for defender: {normalizedName} in {panels.Length} panels");

        foreach (Transform panel in panels)
        {
            //Debug.Log($"[DEBUG] Checking panel: {panel.name}, Child count: {panel.childCount}");
            //Debug.Log($"Checking panel: {panel.name}");
            foreach (Transform child in panel)
            {
                // 跳过包含 TextMeshPro 组件的子对象
                if (child.GetComponent<TMPro.TextMeshProUGUI>() != null)
                {
                    //Debug.Log($"{panel.name} Skipping text object: {child.name}");
                    continue;
                }

                string childName = NormalizeName(child.name);
                //Debug.Log($"Child in panel {panel.name}: {childName}");

                if (childName == normalizedName)
                {
                    //Debug.Log($"Match found: {childName} == {normalizedName}");
                    return true;
                }
            }
        }
        Debug.Log("No match found.");
        return false;
    }

    private bool HasSameDefenderInContainer(string defenderName)
    {
        string normalizedName = NormalizeName(defenderName);
        for (int i = 0; i < containerTransform.childCount; i++)
        {
            Transform defender = containerTransform.GetChild(i);
            if (NormalizeName(defender.name) == normalizedName && defender.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    private string NormalizeName(string name)
    {
        return name.Replace("(Clone)", "").Trim().ToLower();
    }
}
