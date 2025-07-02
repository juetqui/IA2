using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PlayerBackpack : MonoBehaviour
{
    private List<Gem> backpack = new List<Gem>();
    private List<Gem> depositedGems = new List<Gem>(); // Almacena las gemas depositadas
    private float maxWeight = 50f;

    [SerializeField] private EnvironmentAnalyzer environmentAnalyzer;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI gemCountText;
    [SerializeField] private TextMeshProUGUI totalWeightText;
    [SerializeField] private TextMeshProUGUI gemStatsText;
    [SerializeField] private TextMeshProUGUI highValueGemsText;
    [SerializeField] private TextMeshProUGUI gemTypesText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI totalValueText; // Nuevo contador para el valor depositado
    [SerializeField] private TextMeshProUGUI lastActionText;
    [SerializeField] private float warningDisplayTime = 2f;

    [Header("References")]
    [SerializeField] private GemPool gemPool;
    private Coroutine rotatingGemsCoroutine;


    private float warningTimer;

    void Start()
    {
        UpdateUI();
    }

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

        if (Input.GetKeyDown(KeyCode.V))
        {
            DepositTopValuableGems(); // Deposita las top 3
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            environmentAnalyzer.UpdateGemsInScene();
            environmentAnalyzer.UpdateEnvironmentUI();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (rotatingGemsCoroutine != null)
            {
                StopCoroutine(rotatingGemsCoroutine);
            }

            rotatingGemsCoroutine = StartCoroutine(environmentAnalyzer.DisplayGroupedGems());
        }
    }

    /// <summary>
    /// Actualiza la UI con la cantidad de gemas, peso total, estadísticas y valor depositado.
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
            string statsText = "Estadísticas por tipo:\n";
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
    /// Detecta colisiones con gemas y las recolecta si no se excede el peso máximo.
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
                        warningText.text = "¡Mochila llena! Peso máximo alcanzado.";
                        warningTimer = warningDisplayTime;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Procesa la recolección de una gema usando un tipo anónimo para registrar la acción.
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

        if (lastActionText != null)
        {
            lastActionText.text = $"{collectionAction.Action}: {collectionAction.GemType} (Valor: {collectionAction.Value})";
        }
    }

    /// <summary>
    /// Deposita todas las gemas de la mochila en el punto de depósito y actualiza la UI.
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
    /// Agrupa estadísticas de las gemas por tipo (valor total, peso total, cantidad).
    /// </summary>
    /// <returns>Un diccionario con las estadísticas por tipo de gema.</returns>
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
    /// <returns>Una enumeración de gemas de alto valor.</returns>
    private IEnumerable<Gem> GetHighValueGems()
    {
        foreach (var gem in backpack.Where(g => g.Value > 30))
        {
            yield return gem;
        }
    }

    /// <summary>
    /// Obtiene una lista de tipos de gemas únicos (LINQ ToList).
    /// </summary>
    /// <returns>Una lista de tipos de gemas.</returns>
    private List<string> GetGemTypes()
    {
        return backpack.Select(g => g.Type).Distinct().ToList();
    }

    public void DepositTopValuableGems(int count = 3)
    {
        var topGems = GetOrderedGemsByValue().Take(count).ToList();

        backpack.RemoveAll(g => topGems.Contains(g));

        depositedGems.AddRange(topGems);

        if (warningText != null)
        {
            warningText.text = $"Depositaste tus {topGems.Count} gemas más valiosas.";
            warningTimer = warningDisplayTime;
        }

        UpdateUI();
    }

    /// <summary>
    /// Obtiene gemas ordenadas por valor descendente con time-slicing (LINQ OrderBy).
    /// </summary>
    /// <returns>Una enumeración de gemas ordenadas.</returns>
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

}