using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Used to resize child objects with RectTransforms based on certain screen width and height multipliers.
/// </summary>
public class ScaleUIElements : MonoBehaviour
{
    [Header("Scaling options")]
    [Tooltip("All child elements will get their width scaled to that percentage of the screen width.")]
    [SerializeField] private float childWidthScreenRatio;
    [Space(5)]
    [Tooltip("All child elements will get their height scaled to that percentage of the screen height.")]
    [SerializeField] private float childHeightScreenRatio;

    public void Resize()
    {
        RectTransform[] rectArray = GetComponentsInChildren<RectTransform>();
   //     Debug.Log("SCREEN WIDTH: " + Screen.width + "\n" + "SCREEN HEIGHT: " + Screen.height);
        int counter = 0;
        foreach (RectTransform rt in rectArray)
        {
      //      Debug.Log(Screen.height * childHeightScreenRatio);
            if (counter > 0)
                rt.sizeDelta = new Vector2(Screen.width * childWidthScreenRatio, Screen.height * childHeightScreenRatio);


            counter++;
        }
    }
}
