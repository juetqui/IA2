using System.Collections.Generic;
using UnityEngine;

public class StatsPanelManager : MonoBehaviour
{
    [Header("Configuración de paneles")]
    [Tooltip("Lista de pares (Tecla → Panel)")]
    [SerializeField] private List<PanelToggleEntry> panels = new List<PanelToggleEntry>();

    [Header("Referencia al Backpack (para refrescar UI)")]
    [Tooltip("Arrastra aquí tu componente PlayerBackpack")]
    [SerializeField] private PlayerBackpack playerBackpack;

    void Update()
    {
        foreach (var entry in panels)
        {
            if (entry.panel == null)
                continue;

            if (Input.GetKeyDown(entry.key))
            {
                // Alternar visibilidad
                bool isActive = entry.panel.activeSelf;
                entry.panel.SetActive(!isActive);

                // Si lo acabamos de mostrar, refrescamos su contenido
                if (!isActive && playerBackpack != null)
                    playerBackpack.UpdateUI(); // Asegúrate de que UpdateUI() sea público
            }
        }
    }
}