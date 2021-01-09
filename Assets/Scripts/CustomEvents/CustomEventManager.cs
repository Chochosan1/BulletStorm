using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Chochosan
{
    /// <summary>
    /// Holds different custom events. Events here are not capsulated - they can be fired from any place.
    /// </summary>
    public class CustomEventManager : MonoBehaviour
    {
        public delegate void OnUpgradeLearnedDelegate(UpgradeController.UpgradeType upgradeType);
        /// <summary>Event raised when the player receives a new major upgrade. </summary>
        public static OnUpgradeLearnedDelegate OnUpgradeLearned;

        public delegate void OnUpgradePanelRequiredDelegate();
        /// <summary>Event raised when the upgrade panel should open. </summary>
        public static OnUpgradePanelRequiredDelegate OnUpgradePanelRequired;

        public delegate void OnPlayerStatsChangedDelegate(string stats);
        /// <summary>Event raised when the upgrade panel should open. </summary>
        public static OnPlayerStatsChangedDelegate OnPlayerStatsChanged;

        public delegate void OnEnemyKilledDelegate();
        /// <summary>Event raised when the upgrade panel should open. </summary>
        public static OnEnemyKilledDelegate OnEnemyKilled;
    }
}
