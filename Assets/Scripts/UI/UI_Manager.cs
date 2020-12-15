using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] private GameObject upgradesPanel;

    private void Start()
    {
        Chochosan.CustomEventManager.OnUpgradePanelRequired += OpenUpgradePanel;
        Chochosan.CustomEventManager.OnUpgradeLearned += CloseUpgradePanel;
    }

    private void OnDisable()
    {
        Chochosan.CustomEventManager.OnUpgradePanelRequired -= OpenUpgradePanel;
        Chochosan.CustomEventManager.OnUpgradeLearned -= CloseUpgradePanel;
    }

    public void OpenUpgradePanel()
    {
        upgradesPanel.SetActive(true);
        upgradesPanel.GetComponent<ScaleUIElements>().Resize();
    }

    public void CloseUpgradePanel(UpgradeController.UpgradeType upgradeType)
    {
        //maybe display the upgrade type
        upgradesPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            upgradesPanel.SetActive(!upgradesPanel.activeSelf);
            upgradesPanel.GetComponent<ScaleUIElements>().Resize();
        }
    }
}
