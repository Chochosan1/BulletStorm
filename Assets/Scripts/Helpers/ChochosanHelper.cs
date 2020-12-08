using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chochosan
{
    public class ChochosanHelper : MonoBehaviour
    {
        public static void ChochosanDebug(string message, object passedObject, string color)
        {
            switch (color)
            {
                case "red":
                    Debug.Log($"{message} || <color=red> {passedObject.ToString()} </color>");
                    break;
                case "green":
                    Debug.Log($"{message} || <color=teal> {passedObject.ToString()} </color>");
                    break;
            }
        }

        public static void ChochosanDebug(string message, string color)
        {
            switch (color)
            {
                case "red":
                    Debug.Log($"<color=red> {message} </color>");
                    break;
                case "green":
                    Debug.Log($"<color=teal> {message} </color>");
                    break;
            }
        }
    }
}
