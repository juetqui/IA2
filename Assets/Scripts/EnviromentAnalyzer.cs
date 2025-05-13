using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/// <summary>
/// Analiza las gemas activas en la escena, proporcionando estadísticas y filtrados.
/// Permite ejecutar análisis manualmente con la tecla R.
/// </summary>
public class EnvironmentAnalyzer : MonoBehaviour
{
    [SerializeField] private GemPool gemPool;
    [SerializeField] private TextMeshProUGUI environmentStatsText;

    private List<Gem> gemsInScene = new List<Gem>();

    /// <summary>
    /// Inicializa la lista de gemas en la escena.
    /// </summary>
    void Start()
    {
        UpdateGemsInScene();
        UpdateEnvironmentUI();
    }

    /// <summary>
    /// Actualiza la lista de gemas activas en la escena.
    /// </summary>
    private void UpdateGemsInScene()
    {
        gemsInScene.Clear();
        if (gemPool != null)
        {
            foreach (var gemObject in gemPool.GetComponentsInChildren<Gem>(false))
            {
                if (gemObject.gameObject.activeInHierarchy)
                {
                    gemsInScene.Add(gemObject);
                }
            }
        }
    }

    /// <summary>
    /// Agrupa gemas por rangos de valor (bajo, medio, alto) y calcula cantidad y peso promedio.
    /// </summary>
    /// <returns>Un diccionario con estadísticas por rango de valor.</returns>
    private Dictionary<string, (int Count, float AvgWeight)> GetGemValueRanges()
    {
        return gemsInScene
            .GroupBy(g => g.Value < 20 ? "Low" : g.Value < 40 ? "Medium" : "High")
            .ToDictionary(
                group => group.Key,
                group => (
                    Count: group.Count(),
                    AvgWeight: group.Average(g => g.Weight)
                )
            );
    }

    /// <summary>
    /// Obtiene las primeras 5 gemas de la escena (LINQ Take).
    /// </summary>
    /// <returns>Una enumeración de hasta 5 gemas.</returns>
    private IEnumerable<Gem> GetTopGems()
    {
        foreach (var gem in gemsInScene.Take(5))
        {
            yield return gem;
        }
    }

    /// <summary>
    /// Obtiene gemas agrupadas por tipo con time-slicing (LINQ GroupBy).
    /// </summary>
    /// <returns>Una enumeración de gemas agrupadas.</returns>
    private IEnumerable<Gem> GetGemsByType()
    {
        var groupedGems = gemsInScene.GroupBy(g => g.Type).SelectMany(g => g).ToList();
        for (int i = 0; i < groupedGems.Count; i++)
        {
            if (Time.frameCount % 3 == 0)
            {
                yield return groupedGems[i];
            }
        }
    }

    /// <summary>
    /// Verifica si hay gemas con valor mayor a 40 (LINQ Any).
    /// </summary>
    /// <returns>True si hay gemas de alto valor, false en caso contrario.</returns>
    private bool HasHighValueGems()
    {
        return gemsInScene.Any(g => g.Value > 40);
    }

    /// <summary>
    /// Obtiene la gema más cercana (simulada con selección aleatoria) como tupla.
    /// </summary>
    /// <returns>Una tupla con tipo, valor y distancia simulada.</returns>
    private (string Type, int Value, float Distance) GetClosestGem()
    {
        var gem = gemsInScene.OrderBy(g => Random.value).FirstOrDefault();
        return (gem?.Type ?? "None", gem?.Value ?? 0, Random.Range(1f, 10f));
    }

    /// <summary>
    /// Analiza la escena y actualiza la UI del entorno.
    /// </summary>
    private void AnalyzeScene()
    {
        UpdateGemsInScene();
        UpdateEnvironmentUI();
    }

    /// <summary>
    /// Actualiza la UI con estadísticas del entorno.
    /// </summary>
    private void UpdateEnvironmentUI()
    {
        if (environmentStatsText != null)
        {
            var ranges = GetGemValueRanges();
            string statsText = $"Entorno ({gemsInScene.Count} gemas):\n";
            statsText += $"Gemas valiosas (>40): {(HasHighValueGems() ? "Sí" : "No")}\n";
            foreach (var range in ranges)
            {
                statsText += $"{range.Key}: {range.Value.Count} gemas, Peso promedio: {range.Value.AvgWeight:F1}\n";
            }
            var closestGem = GetClosestGem();
            statsText += $"Gema más cercana: {closestGem.Type} (Valor: {closestGem.Value}, Distancia: {closestGem.Distance:F1})";
            environmentStatsText.text = statsText;
        }
    }

    /// <summary>
    /// Ejecuta el análisis de la escena al presionar la tecla R.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            AnalyzeScene();
        }
    }
}