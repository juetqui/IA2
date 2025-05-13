using UnityEngine;

/// <summary>
/// Representa una gema en el juego, con propiedades como tipo, valor y peso.
/// Permite inicializar la gema y desactivarla.
/// </summary>
public class Gem : MonoBehaviour
{
    public string Type { get; private set; }
    public int Value { get; private set; }
    public float Weight { get; private set; }

    /// <summary>
    /// Inicializa la gema con un tipo, valor y peso específicos.
    /// Cambia el color del material según el tipo de gema.
    /// </summary>
    /// <param name="type">Tipo de gema (Red, Blue, Green).</param>
    /// <param name="value">Valor en puntos de la gema.</param>
    /// <param name="weight">Peso de la gema.</param>
    public void Initialize(string type, int value, float weight)
    {
        Type = type;
        Value = value;
        Weight = weight;

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = type switch
            {
                "Red" => Color.red,
                "Blue" => Color.blue,
                "Green" => Color.green,
                _ => Color.white
            };
        }
    }

    /// <summary>
    /// Desactiva la gema en la escena.
    /// </summary>
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}