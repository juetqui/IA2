using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona la mochila del jugador, permitiendo recolectar y depositar gemas, actualizar la UI,
/// y realizar operaciones de agregaci�n y filtrado con LINQ.
/// </summary>
public class PlayerBackpack : MonoBehaviour
{
    private List<Gem> backpack = new List<Gem>();
    private List<Gem> depositedGems = new List<Gem>(); // Almacena las gemas depositadas
    private float maxWeight = 50f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI gemCountText;
    [SerializeField] private TextMeshProUGUI totalWeightText;
    [SerializeField] private TextMeshProUGUI gemStatsText;
    [SerializeField] private TextMeshProUGUI highValueGemsText;
    [SerializeField] private TextMeshProUGUI gemTypesText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI totalValueText; // Nuevo contador para el valor depositado
    [SerializeField] private float warningDisplayTime = 2f;

    [Header("References")]
    [SerializeField] private GemPool gemPool;

    private float warningTimer;

    /// <summary>
    /// Inicializa la UI de la mochila.
    /// </summary>
    void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Actualiza el temporizador para el mensaje de advertencia.
    /// </summary>
    void Update()
    {
        if (warningTimer > 0)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0 && warningText != null)
            {
                warningText.text = "";
            }
        }
    }

    /// <summary>
    /// Detecta colisiones con gemas y las recolecta si no se excede el peso m�ximo.
    /// </summary>
    /// <param name="other">El collider del objeto colisionado.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Gem"))
        {
            Gem gem = other.gameObject.GetComponent<Gem>();
            if (gem != null)
            {
                float currentWeight = backpack.Sum(g => g.Weight);
                if (currentWeight + gem.Weight <= maxWeight)
                {
                    backpack.Add(gem);
                    ProcessGemCollection(gem);
                    gem.Deactivate();
                    if (gemPool != null)
                    {
                        gemPool.SpawnGem();
                    }
                    UpdateUI();
                }
                else
                {
                    if (warningText != null)
                    {
                        warningText.text = "�Mochila llena! Peso m�ximo alcanzado.";
                        warningTimer = warningDisplayTime;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Deposita todas las gemas de la mochila en el punto de dep�sito y actualiza la UI.
    /// </summary>
    public void DepositGems()
    {
        if (backpack.Count > 0)
        {
            depositedGems.AddRange(backpack);
            backpack.Clear();
            if (warningText != null)
            {
                warningText.text = "Gemas depositadas correctamente.";
                warningTimer = warningDisplayTime;
            }
            UpdateUI();
        }
        else
        {
            if (warningText != null)
            {
                warningText.text = "No hay gemas para depositar.";
                warningTimer = warningDisplayTime;
            }
        }
    }

    /// <summary>
    /// Actualiza la UI con la cantidad de gemas, peso total, estad�sticas y valor depositado.
    /// </summary>
    private void UpdateUI()
    {
        if (gemCountText != null)
        {
            gemCountText.text = $"Gemas: {backpack.Count}";
        }

        if (totalWeightText != null)
        {
            totalWeightText.text = $"Peso: {backpack.Sum(g => g.Weight):F1} / {maxWeight}";
        }

        if (gemStatsText != null)
        {
            var stats = GetGemStats();
            string statsText = "Estad�sticas por tipo:\n";
            foreach (var stat in stats)
            {
                statsText += $"{stat.Key}: {stat.Value.Count} gemas, Valor: {stat.Value.TotalValue}, Peso: {stat.Value.TotalWeight:F1}\n";
            }
            gemStatsText.text = statsText;
        }

        if (highValueGemsText != null)
        {
            int highValueCount = GetHighValueGems().Count();
            highValueGemsText.text = $"Gemas valiosas (>30): {highValueCount}";
        }

        if (gemTypesText != null)
        {
            var types = GetGemTypes();
            gemTypesText.text = $"Tipos recolectados: {string.Join(", ", types)}";
        }

        if (totalValueText != null)
        {
            int totalValue = depositedGems.Sum(g => g.Value);
            totalValueText.text = $"Valor depositado: {totalValue}";
        }
    }

    /// <summary>
    /// Agrupa estad�sticas de las gemas por tipo (valor total, peso total, cantidad).
    /// </summary>
    /// <returns>Un diccionario con las estad�sticas por tipo de gema.</returns>
    private Dictionary<string, (int TotalValue, float TotalWeight, int Count)> GetGemStats()
    {
        return backpack
            .GroupBy(g => g.Type)
            .ToDictionary(
                group => group.Key,
                group => (
                    TotalValue: group.Sum(g => g.Value),
                    TotalWeight: group.Sum(g => g.Weight),
                    Count: group.Count()
                )
            );
    }

    /// <summary>
    /// Obtiene gemas con valor mayor a 30 usando un generador (LINQ Where).
    /// </summary>
    /// <returns>Una enumeraci�n de gemas de alto valor.</returns>
    private IEnumerable<Gem> GetHighValueGems()
    {
        foreach (var gem in backpack.Where(g => g.Value > 30))
        {
            yield return gem;
        }
    }

    /// <summary>
    /// Obtiene gemas ordenadas por valor descendente con time-slicing (LINQ OrderBy).
    /// </summary>
    /// <returns>Una enumeraci�n de gemas ordenadas.</returns>
    private IEnumerable<Gem> GetOrderedGemsByValue()
    {
        var orderedGems = backpack.OrderByDescending(g => g.Value).ToList();
        for (int i = 0; i < orderedGems.Count; i++)
        {
            if (Time.frameCount % 2 == 0)
            {
                yield return orderedGems[i];
            }
        }
    }

    /// <summary>
    /// Obtiene una lista de tipos de gemas �nicos (LINQ ToList).
    /// </summary>
    /// <returns>Una lista de tipos de gemas.</returns>
    private List<string> GetGemTypes()
    {
        return backpack.Select(g => g.Type).Distinct().ToList();
    }

    /// <summary>
    /// Registra un evento de recolecci�n de gema como una tupla.
    /// </summary>
    /// <param name="gem">La gema recolectada.</param>
    /// <returns>Una tupla con tipo, valor, peso y timestamp.</returns>
    private (string GemType, int Value, float Weight, float Timestamp) RecordCollection(Gem gem)
    {
        return (gem.Type, gem.Value, gem.Weight, Time.time);
    }

    /// <summary>
    /// Procesa la recolecci�n de una gema usando un tipo an�nimo para registrar la acci�n.
    /// </summary>
    /// <param name="gem">La gema recolectada.</param>
    private void ProcessGemCollection(Gem gem)
    {
        var collectionAction = new
        {
            Action = "Collected",
            GemType = gem.Type,
            Value = gem.Value,
            Weight = gem.Weight,
            Timestamp = Time.time
        };
    }
}