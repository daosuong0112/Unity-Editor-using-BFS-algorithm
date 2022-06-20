using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Style : MonoBehaviour
{
    public ButtonStyle[] button;
}
[System.Serializable]
public struct ButtonStyle
{
    public Texture2D Icon;
    public string ButtonText;
    public GameObject PrefabsGameObj;
    [HideInInspector]
    public GUIStyle CellStyle;
}
