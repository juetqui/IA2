using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Aggregate root para estadísticas de gemas a lo largo de toda la sesión de juego.
/// </summary>
public class SessionGemStats
{
    // Contadores y acumuladores
    private int totalCollectedCount = 0;
    private int totalDepositedCount = 0;
    private float totalCollectedValue = 0f;
    private float totalCollectedWeight = 0f;
    private float maxCollectedWeight = 0f;

    // Conteo por tipo de gema
    private Dictionary<string, int> collectedByType = new Dictionary<string, int>();
    private Dictionary<string, int> depositedByType = new Dictionary<string, int>();

    /// <summary>
    /// Registra una gema recogida en la sesión.
    /// </summary>
    public void RegisterCollected(Gem gem)
    {
        totalCollectedCount++;
        totalCollectedValue += gem.Value;
        totalCollectedWeight += gem.Weight;
        maxCollectedWeight = Math.Max(maxCollectedWeight, gem.Weight);

        if (collectedByType.ContainsKey(gem.Type))
            collectedByType[gem.Type]++;
        else
            collectedByType[gem.Type] = 1;
    }

    /// <summary>
    /// Registra un conjunto de gemas depositadas en la sesión.
    /// </summary>
    public void RegisterDeposited(IEnumerable<Gem> gems)
    {
        var list = gems.ToList();
        totalDepositedCount += list.Count;
        foreach (var gem in list)
        {
            if (depositedByType.ContainsKey(gem.Type))
                depositedByType[gem.Type]++;
            else
                depositedByType[gem.Type] = 1;
        }
    }

    // Propiedades de sólo lectura para exponer las métricas:
    public int TotalCollectedCount     => totalCollectedCount;
    public int TotalDepositedCount     => totalDepositedCount;
    public float TotalCollectedValue   => totalCollectedValue;
    public float TotalCollectedWeight  => totalCollectedWeight;
    public float MaxCollectedWeight    => maxCollectedWeight;
    public float AverageCollectedValue => 
        totalCollectedCount > 0 ? totalCollectedValue / totalCollectedCount : 0f;

    /// <summary>
    /// Tipo de gema más frecuente en las recogidas.
    /// </summary>
    public string MostFrequentCollectedType =>
        collectedByType
            .OrderByDescending(kv => kv.Value)
            .FirstOrDefault()
            .Key ?? "None";

    /// <summary>
    /// Tipo de gema más frecuente en los depósitos.
    /// </summary>
    public string MostFrequentDepositedType =>
        depositedByType
            .OrderByDescending(kv => kv.Value)
            .FirstOrDefault()
            .Key ?? "None";
}
