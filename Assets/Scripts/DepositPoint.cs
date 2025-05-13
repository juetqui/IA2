using UnityEngine;

/// <summary>
/// Representa un punto de dep�sito en la escena donde el jugador puede depositar gemas.
/// Notifica a PlayerBackpack cuando se activa la acci�n de dep�sito.
/// </summary>
public class DepositPoint : MonoBehaviour
{
    [SerializeField] private KeyCode depositKey = KeyCode.E;
    private bool playerInRange;

    /// <summary>
    /// Detecta cuando el jugador entra en el trigger.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    /// <summary>
    /// Detecta cuando el jugador sale del trigger.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    /// <summary>
    /// Verifica si el jugador presiona la tecla de dep�sito mientras est� en rango.
    /// </summary>
    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(depositKey))
        {
            var playerBackpack = FindObjectOfType<PlayerBackpack>();
            if (playerBackpack != null)
            {
                playerBackpack.DepositGems();
            }
        }
    }
}