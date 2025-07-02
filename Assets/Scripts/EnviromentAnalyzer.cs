using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class EnvironmentAnalyzer : MonoBehaviour
{
    [SerializeField] private GemPool gemPool;
    [SerializeField] private TextMeshProUGUI environmentStatsText;
    [SerializeField] private TextMeshProUGUI topGemsText;
    [SerializeField] private TextMeshProUGUI groupedGemsText;

    private List<Gem> gemsInScene = new List<Gem>();
    void Start()
    {
        UpdateGemsInScene();
        UpdateEnvironmentUI();
    }

    /// <summary>
    /// Actualiza la lista de gemas activas en la escena.
    /// </summary>
    public void UpdateGemsInScene()
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
    /// Actualiza la UI con estadísticas del entorno.
    /// </summary>
    public void UpdateEnvironmentUI()
    {
        if (environmentStatsText != null)
        {
            UpdateTopGemsUI();
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

    //LINQ
    private bool HasHighValueGems() => gemsInScene.Take(5).OrderByDescending(g =>g.Value).Any(g => g.Value > 40);

    //TUPLA
    private (string Type, int Value, float Distance) GetClosestGem()
    {
        var gem = gemsInScene.OrderByDescending(g => Random.value).ToList().FirstOrDefault();          
        return (gem?.Type ?? "None", gem?.Value ?? 0, Random.Range(1f, 10f));
    }

    private void UpdateTopGemsUI()
    {
        if (topGemsText == null) return;

        var topGems = GetTopGems().ToList();
        if (topGems.Count == 0)
        {
            topGemsText.text = "Top 5 gemas: ninguna activa.";
            return;
        }

        string info = "Top 5 gemas:\n";
        foreach (var gem in topGems)
        {
            info += $"- {gem.Type} (Valor: {gem.Value}, Peso: {gem.Weight:F1})\n";
        }

        topGemsText.text = info;
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

    public IEnumerator<WaitForSeconds> DisplayGroupedGems()
    {
        groupedGemsText.text = "Agrupando gemas por tipo...\n";

        foreach (var gem in GetGemsByType())
        {
            groupedGemsText.text += $"- {gem.Type}: Valor {gem.Value}, Peso {gem.Weight:F1}\n";
            yield return new WaitForSeconds(0.1f); // una gema cada 0.1 seg
        }

        groupedGemsText.text += "\nFin del escaneo.";
    }


    /// <summary>
    /// Obtiene gemas agrupadas por tipo con time-slicing (LINQ GroupBy).
    /// </summary>
    /// <returns>Una enumeración de gemas agrupadas.</returns>
    private IEnumerable<Gem> GetGemsByType()
    {
        var groupedGems = gemsInScene.GroupBy(g => g.Type).SelectMany(g => g).Take(5).ToList();
        for (int i = 0; i < groupedGems.Count; i++)
        {
            if (Time.frameCount % 3 == 0)
            {
                yield return groupedGems[i];
            }
        }
    }
}