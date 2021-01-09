using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject unlockedUpgradesPanel;

    [Header("Buttons")]
    [SerializeField] private List<GameObject> allButtons;

    [Header("Hover materials")]
    [SerializeField] private Material normalMat;
    [SerializeField] private Material highlightMat;

    [Header("Stats UI")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI currentHealthText;
    [SerializeField] private TextMeshProUGUI shootRateText;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Chochosan.CustomEventManager.OnUpgradePanelRequired += OpenUpgradePanel;
        Chochosan.CustomEventManager.OnUpgradeLearned += CloseUpgradePanel;
        Chochosan.CustomEventManager.OnPlayerStatsChanged += OnPlayerStatsChanged;
    }

    private void OnDisable()
    {
        Chochosan.CustomEventManager.OnUpgradePanelRequired -= OpenUpgradePanel;
        Chochosan.CustomEventManager.OnUpgradeLearned -= CloseUpgradePanel;
        Chochosan.CustomEventManager.OnPlayerStatsChanged -= OnPlayerStatsChanged;
    }

    public void OpenUpgradePanel()
    {
        upgradesPanel.SetActive(true);

        CalculateUpgradeButtonsToShow();

        upgradesPanel.GetComponent<ScaleUIElements>().Resize();
    }

    public void CloseUpgradePanel(UpgradeController.UpgradeType upgradeType)
    {
        foreach (Button btn in upgradesPanel.GetComponentsInChildren<Button>())
        {
            btn.gameObject.SetActive(false);
        }

        //maybe display the upgrade type
        upgradesPanel.SetActive(false);
    }

    public void OnUpgradeButtonHover(Image image)
    {
        image.material = highlightMat;
    }

    public void OnUpgradeButtonLeaveHover(Image image)
    {
        image.material = normalMat;
    }

    private void OnPlayerStatsChanged(string stats)
    {
        switch (stats)
        {
            case "maxHealth":
                maxHealthText.text = PlayerController.Instance.MaxHealth.ToString("F0");
                break;
            case "currentHealth":
                currentHealthText.text = PlayerController.Instance.CurrentHealth.ToString("F0");
                break;
            case "attackDamage":
                attackDamageText.text = PlayerController.Instance.AttackDamage.ToString("F1");
                break;
            case "shootRate":
                shootRateText.text = PlayerController.Instance.ShootRate.ToString("F1");
                break;
            case "enemiesKilled":
                enemiesKilledText.text = PlayerController.Instance.EnemiesKilled.ToString();
                break;
        }

    }

    #region HandleRandomUpgradeButtonDisplay
    private void CalculateUpgradeButtonsToShow()
    {
        //how many upgrades to show?
        int upgradesToShow = Random.Range(2, 5);

        //keeps track of what buttons have been shown in the current iteration so that the algorithm does not try to enable already enabled buttons
        int[] chosenButtons = new int[upgradesToShow];

        //set everything to -1 because having a 0 inside from the start will break the check logic below
        for (int i = 0; i < chosenButtons.Length; i++)
        {
            chosenButtons[i] = -1;
        }

        for (int i = 0; i < upgradesToShow; i++)
        {
            if (allButtons.Count == 0)
            {
                Debug.Log("MAX NUMBER OF UPGRADES REACHED!");
                return;
            }


            int chosenBtn = Random.Range(0, allButtons.Count);

            //if the randomly chosen button has already been shown in this iteration 
            //then go to the next button or reset to the first element if out of bounds
            for (int j = 0; j < chosenButtons.Length; j++)
            {
                if (chosenBtn == chosenButtons[j])
                    chosenBtn++;
            }
            if (chosenBtn >= allButtons.Count)
                chosenBtn = 0;


            allButtons[chosenBtn].SetActive(true);
            chosenButtons[i] = chosenBtn;
        }
    }

    ///<summary>Removes the button from the list with all upgrade buttons so that it is not chosen again to show to the player. Called on-click.</summary>
    public void RemoveUpgradeButtonFromList(GameObject btn)
    {
        allButtons.Remove(btn);
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            upgradesPanel.SetActive(!upgradesPanel.activeSelf);
            upgradesPanel.GetComponent<ScaleUIElements>().Resize();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            unlockedUpgradesPanel.SetActive(!unlockedUpgradesPanel.activeSelf);
        }
        else if(Input.GetKeyDown(KeyCode.I))
        {
            statsPanel.SetActive(!statsPanel.activeSelf);
        }
    }
}
