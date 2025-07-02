// Gem.cs
using System;
using UnityEngine;

/// <summary>
/// Representa una gema en el juego, con propiedades como tipo, valor y peso.
/// Emite un evento cuando se desactiva para que otros sistemas puedan suscribirse.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Gem : MonoBehaviour
{
    public string Type   { get; private set; }
    public int    Value  { get; private set; }
    public float  Weight { get; private set; }

    /// <summary>
    /// Se dispara cuando la gema se desactiva (por ejemplo, al depositarse).
    /// </summary>
    public static event Action<Gem> OnGemDeactivated;

    /// <summary>
    /// Inicializa la gema con valores específicos y resetea su transform.
    /// </summary>
    /// <param name="type">“Red”, “Blue”, “Green”, “Rare” u otro.</param>
    /// <param name="value">Valor en puntos (>= 0).</param>
    /// <param name="weight">Peso (> 0).</param>
    public virtual void Initialize(string type, int value, float weight)
    {
        // Validación mínima
        if (value < 0 || weight <= 0f)
            Debug.LogWarning($"[Gem] Valores inválidos: value={value}, weight={weight}");

        // Reset de transform
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale    = Vector3.one;

        Type   = type;
        Value  = value;
        Weight = weight;

        // Color según tipo
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (type)
            {
                case "Red":    renderer.material.color = Color.red;    break;
                case "Blue":   renderer.material.color = Color.blue;   break;
                case "Green":  renderer.material.color = Color.green;  break;
                case "Rare":   renderer.material.color = Color.yellow; break;
                default:       renderer.material.color = Color.white;  break;
            }
        }
    }

    /// <summary>
    /// Desactiva la gema y notifica a los suscriptores.
    /// </summary>
    public void Deactivate()
    {
        OnGemDeactivated?.Invoke(this);
        gameObject.SetActive(false);
    }
}
