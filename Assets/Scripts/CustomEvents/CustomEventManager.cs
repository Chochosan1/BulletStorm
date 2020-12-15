using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Chochosan
{
    public class CustomEventManager : MonoBehaviour
    {
        public delegate void OnUpgradeLearnedDelegate(UpgradeController.UpgradeType upgradeType);
        /// <summary>Event raised when the player receives a new major upgrade. </summary>
        public static OnUpgradeLearnedDelegate OnUpgradeLearned;

        public delegate void OnUpgradePanelRequiredDelegate();
        /// <summary>Event raised when the upgrade panel should open. </summary>
        public static OnUpgradePanelRequiredDelegate OnUpgradePanelRequired;
    }
}
