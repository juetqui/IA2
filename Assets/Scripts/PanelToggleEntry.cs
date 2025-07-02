using UnityEngine;

[System.Serializable]
public struct PanelToggleEntry
{
    [Tooltip("Tecla para alternar este panel")]
    public KeyCode key;

    [Tooltip("Panel (GameObject) que se mostrará/ocultará")]
    public GameObject panel;
}