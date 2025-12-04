using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using Button = UnityEngine.UI.Button;

public class MenuButton : MonoBehaviour
{
    public Button battleButton;
    public Button upgradeButton;
    public Button editTeamButton;
    public Button back;
    public Button backFromTeam;
    public Button editTeamInUpgradeButton;
    public Button closeEquipmentCanvas;
    public Canvas upgradeCanvas;
    public Canvas editTeamCanvas;
    public Canvas equipmentCanvas;
    //public Button clearPlayerPrefsButton; // 清空 PlayerPrefs 的按钮
    public GameObject fortuneWheel;


    public Button drawCardButton;
    public Button drawGearButton;
    public Canvas drawGearCanvas;
    public Button closeDrawGearCanvas;
    //public Button closeDrawCardButton;
    //public Button drawCardsMutipleButton;

    public Transform editTeamContainer; // Edit Team 容器
    public Transform editTeamPanelContainer; // Edit Team 容器
    public Transform defenderContainer; // defender container

    public LotterySystem lotterySystem;



    // Start is called before the first frame update
    void Start()
    {
        upgradeCanvas.enabled = false;
        editTeamCanvas.enabled = false;
        // 绑定点击事件到按钮
        battleButton.onClick.AddListener(OnBattleClicked);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        editTeamButton.onClick.AddListener(OnEditTeamClicked);
        back.onClick.AddListener(OnBackClicked);
        backFromTeam.onClick.AddListener(OnBackFromTeamClicked);
        editTeamButton.onClick.AddListener(DisableEditTeamUnnecessaryComponents);
        editTeamInUpgradeButton.onClick.AddListener (OnEditTeamInUpgradeClicked);
        closeEquipmentCanvas.onClick.AddListener(onCloseEquipmentCanvasClicked);
        //clearPlayerPrefsButton.onClick.AddListener(ClearPlayerPrefs);
        drawCardButton.onClick.AddListener(OnDrawCardButtonClicked);
        //closeDrawCardButton.onClick.AddListener(OnCloseDrawCardButtonClicked);
        //drawCardsMutipleButton.onClick.AddListener(OnDrawCarsMutipleButtonClicked);
        drawGearButton.onClick.AddListener(OnDrawGearButtonClicked);
        closeDrawGearCanvas.onClick.AddListener(OnCloseDrawGearCanvasClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnBattleClicked()
    {
        AudioManager.instance.PlayClickButtonSound();
        SceneManager.LoadScene("MapScene");
    }

    // 清空 PlayerPrefs 的方法
    //public void ClearPlayerPrefs()
    //{
    //    Debug.Log("Clearing PlayerPrefs...");
    //    PlayerPrefs.DeleteAll();
    //    PlayerPrefs.Save();
    //    Debug.Log("PlayerPrefs cleared successfully.");

       
    //    // 重新加载场景，确保数据刷新
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    //}


    private void OnUpgradeClicked()
    {
        AudioManager.instance.PlayClickButtonSound();
        upgradeCanvas.enabled = true;
        editTeamCanvas.enabled = false;

        foreach (Transform defender in defenderContainer.transform)
        {
            CanvasGroup canvasGroup = defender.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = defender.AddComponent<CanvasGroup>();
            }
            canvasGroup.blocksRaycasts = false; // 禁用 Raycast
        }
    }

    private void OnEditTeamClicked()
    {
        //GameManager.Instance.RestorePanelDefenders();
        AudioManager.instance.PlayClickButtonSound();
        // 禁用整个 Canvas GameObject
        editTeamCanvas.gameObject.SetActive(true);
        editTeamCanvas.enabled = true;
        upgradeCanvas.enabled = false;
        //Debug.Log("Edit Team clicked");
    }

    private void OnEditTeamInUpgradeClicked()
    {
        AudioManager.instance.PlayClickButtonSound();
        // 禁用整个 Canvas GameObject
        editTeamCanvas.gameObject.SetActive(true);
        editTeamCanvas.enabled = true;
        upgradeCanvas.enabled = false;
    }


    private void OnBackClicked()
    {
        AudioManager.instance.PlayClickButtonSound();
        upgradeCanvas.enabled = false;

    }

    private void OnBackFromTeamClicked()
    {
        AudioManager.instance.PlayClickButtonSound();
        // 禁用整个 Canvas GameObject
        editTeamCanvas.gameObject.SetActive(false);
        editTeamCanvas.enabled = false;
    }





    private void OnDrawCardButtonClicked()
    {
        fortuneWheel.SetActive(true);
    }

    private void OnDrawGearButtonClicked()
    {
        drawGearCanvas.gameObject.SetActive(true);
        
    }

    private void OnCloseDrawGearCanvasClicked()
    {
        drawGearCanvas.gameObject.SetActive(false);
    }

    //private void OnCloseDrawCardButtonClicked()
    //{
    //    fortuneWheel.SetActive(false);
    //}

    //private void OnDrawCarsMutipleButtonClicked()
    //{
    //    lotterySystem.DrawDefender();

    //}

    //清除edit Team里面不必要的内容
    public void DisableEditTeamUnnecessaryComponents()
    {
        //Debug.Log("🛠️ DisableEditTeamUnnecessaryComponents() 开始执行");

        // 遍历 editTeamContainer 里的 defender
        foreach (Transform defender in editTeamContainer)
        {
            //Debug.Log($"🔎 正在处理 EditTeamContainer 内的 Defender: {defender.name}");
            ProcessDefender(defender);
        }

        // 遍历 Panel 里的所有 PanelX，并查找 Defender
        Transform panelContainer = GameObject.Find("Panel").transform; // 确保 "Panel" 是正确的父对象名
        if (panelContainer == null)
        {
            Debug.LogError("❌ Panel 组件未找到，检查 GameObject 名称是否正确！");
            return;
        }

        foreach (Transform panel in panelContainer)
        {
            //Debug.Log($"📂 正在处理 Panel: {panel.name}");
            foreach (Transform defender in panel) // 获取 PanelX 里面的 defender
            {
                //Debug.Log($"🔎 处理 Panel: {panel.name} 内的 Defender: {defender.name}");
                ProcessDefender(defender);
            }
        }

        //Debug.Log("Unnecessary components in Edit Team Container and Panel have been disabled.");
    }

    // **处理单个 defender**
    private void ProcessDefender(Transform defender)
    {
        //Debug.Log($"⚙️ 开始处理 Defender: {defender.name}");
        // **禁用升级按钮**
        Button upgradeButton = defender.GetComponentInChildren<Button>();
        if (upgradeButton != null)
        {
            //Debug.Log($"🔴 禁用 {defender.name} 的升级按钮");
            upgradeButton.interactable = false;
            upgradeButton.gameObject.SetActive(false);
        }

        // **隐藏特定子对象**
        string[] childNamesToDisable = new string[] {
        "Square", "AttackPower", "Health", "Level", "LevelTitle",
        "IconSword", "IconHeart", "Name", "IconLV", "background"
    };

        foreach (string childName in childNamesToDisable)
        {
            Transform child = FindInChildren(defender, childName);
            if (child != null)
            {
                //Debug.Log($"👀 找到 {childName} 并隐藏");
                child.gameObject.SetActive(false);
            }
            else
            {
                //Debug.LogWarning($"Child object {childName} not found in {defender.name}");
            }
        }

        // **延迟启用 DraggableDefender**
        DraggableDefender draggable = defender.GetComponent<DraggableDefender>();
        if (draggable != null)
        {
            //Debug.Log($"🟢 发现 DraggableDefender 组件，准备延迟启用: {defender.name}");
            StartCoroutine(EnableDraggableAfterDelay(draggable, 0f));
        }
        else
        {
            Debug.LogWarning($"DraggableDefender script not found on {defender.name}");
        }
    }
    


    // **延迟启用 DraggableDefender**
    private IEnumerator EnableDraggableAfterDelay(DraggableDefender draggable, float delay)
    {
        if (draggable != null)
        {
            //Debug.Log($"在延迟启动前发现{draggable}");
            
        }

        //Debug.Log($"⏳ 等待 {delay} 秒后启用 DraggableDefender: {draggable.gameObject.name}");
        yield return new WaitForSeconds(delay);

        if (draggable == null)
        {
            Debug.LogError("❌ DraggableDefender 组件在延迟期间变为 NULL，可能已被销毁！");
            yield break;
        }

        if (draggable != null)
        {
            //Debug.Log($"在延迟启动后发现{draggable}");

        }

        if (draggable.gameObject == null)
        {
            Debug.LogError("❌ DraggableDefender 的 GameObject 已经被销毁！");
            yield break;
        }

        if (!draggable.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"⚠️ {draggable.gameObject.name} 被 SetActive(false)，尝试重新启用...");
            //draggable.gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();  // **等待一帧，确保 GameObject 被激活**
        }
        // 🔥 **再次确认 draggable 仍然有效**
        if (draggable == null || draggable.gameObject == null)
        {
            Debug.LogError("❌ 在 SetActive(true) 之后，DraggableDefender 仍然为空，可能已被销毁！");
            yield break;
        }
        draggable.enabled = true;
        //Debug.Log($"✅ DraggableDefender 重新启用: {draggable.gameObject.name}");
    }

    // 递归查找子对象方法
    private Transform FindInChildren(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindInChildren(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }

    private void onCloseEquipmentCanvasClicked()
    {
        equipmentCanvas.gameObject.SetActive(false);

        //// **重新加载 Defender 数据**
        //DefenderDataManager.Instance.LoadData();

        //// 从 Resources/Defenders 文件夹加载所有 Defender，包括 Tower 和 Meat
        //Defender[] defenders = Resources.LoadAll<Defender>("Defenders");

        //foreach (var defender in defenders)
        //{
        //    defender.RecalculateState();
        //}


        
    }



}
