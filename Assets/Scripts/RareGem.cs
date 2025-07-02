// RareGem.cs
using UnityEngine;

/// <summary>
/// Gema de categoría “rara”. Hereda de Gem y aplica un estilo especial (color fucsia).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class RareGem : Gem
{
    public override void Initialize(string type, int value, float weight)
    {
        // Llamamos al base para el reset de transform y asignación de Type, Value, Weight
        base.Initialize(type, value, weight);

        // Escala especial
        transform.localScale *= 1.5f;

        // Color fucsia
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.magenta;
            // Opcional: añadir emisión para brillo
            renderer.material.EnableKeyword("_EMISSION");
        }
    }
}