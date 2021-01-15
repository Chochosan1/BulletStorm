using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attached to a start cell. Will find the player and move him to the cell's spawn point (will not instantiate, just move).
/// </summary>
public class StartCell : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject[] bigUpgradeOrbs;
    [SerializeField] private float enableBonusBigOrbs = 0.1f;

    void Start()
    {
        Debug.Log("(START)SHOULD TELEPORT PLAYER");
        GameObject player = PlayerController.Instance.gameObject;
        player.transform.position = playerSpawnPoint.position;

        CameraFollowTarget.Instance.SetCameraTarget(player.transform);
        ToggleBigUpgradeOrbs();   
    }

    private void OnLevelWasLoaded(int level)
    {
        Debug.Log("(LEVELLOADED)SHOULD TELEPORT PLAYER");
        GameObject player = PlayerController.Instance.gameObject;
        player.transform.position = playerSpawnPoint.position;

        CameraFollowTarget.Instance.SetCameraTarget(player.transform);

      //  Destroy(this);
    }

    private void ToggleBigUpgradeOrbs()
    {
        foreach (GameObject orb in bigUpgradeOrbs)
        {
            float chanceRolled = Random.Range(0f, 1f);

            if (chanceRolled >= enableBonusBigOrbs)
            {
                orb.SetActive(false);
            }
        }
    }
}
