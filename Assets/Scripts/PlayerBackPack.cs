using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PlayerBackpack : MonoBehaviour
{
    private List<Gem> backpack = new List<Gem>();
    private List<Gem> depositedGems = new List<Gem>();
    private float maxWeight = 50f;

    [SerializeField] private EnvironmentAnalyzer environmentAnalyzer;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI gemCountText;
    [SerializeField] private TextMeshProUGUI totalWeightText;
    [SerializeField] private TextMeshProUGUI gemStatsText;
    [SerializeField] private TextMeshProUGUI highValueGemsText;
    [SerializeField] private TextMeshProUGUI gemTypesText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI totalValueText;
    [SerializeField] private TextMeshProUGUI lastActionText;
    [SerializeField] private float warningDisplayTime = 2f;
    [SerializeField] private TextMeshProUGUI rareGemCountText;
    [SerializeField] private TextMeshProUGUI rareGemDepositedText;

    [Header("Session Stats UI")]
    [SerializeField] private TextMeshProUGUI totalCollectedText;
    [SerializeField] private TextMeshProUGUI totalDepositedText;
    [SerializeField] private TextMeshProUGUI avgValueText;
    [SerializeField] private TextMeshProUGUI maxWeightTextUI;
    [SerializeField] private TextMeshProUGUI mostFreqCollectedText;
    [SerializeField] private TextMeshProUGUI mostFreqDepositedText;

    [Header("References")]
    [SerializeField] private GemPool gemPool;

    private SessionGemStats sessionStats = new SessionGemStats();
    private Coroutine rotatingGemsCoroutine;
    private float warningTimer;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0f && warningText != null)
                warningText.text = "";
        }

        if (Input.GetKeyDown(KeyCode.V))
            DepositTopValuableGems();

        if (Input.GetKeyDown(KeyCode.R))
        {
            environmentAnalyzer.UpdateEnvironmentUI();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (rotatingGemsCoroutine != null)
                StopCoroutine(rotatingGemsCoroutine);
            rotatingGemsCoroutine = StartCoroutine(environmentAnalyzer.DisplayGroupedGems());
        }
    }

    public void UpdateUI()
    {
        
        var bagSummary = backpack.Aggregate(
            (Count: 0, TotalValue: 0, TotalWeight: 0f),
            (acc, gem) => (
                acc.Count + 1,
                acc.TotalValue + gem.Value,
                acc.TotalWeight + gem.Weight
            )
        );

        if (gemCountText != null)
            gemCountText.text = $"Gemas: {bagSummary.Count}";

        if (totalWeightText != null)
            totalWeightText.text = $"Peso: {bagSummary.TotalWeight:F1} / {maxWeight}";

        if (gemStatsText != null)
        {
            var stats = GetGemStats();
            string statsText = "Estadísticas por tipo:\n";
            foreach (var stat in stats)
                statsText += $"{stat.Key}: {stat.Value.Count} gemas, Valor: {stat.Value.TotalValue}, Peso: {stat.Value.TotalWeight:F1}\n";
            gemStatsText.text = statsText;
        }

        if (rareGemCountText != null)
        {
            int rareInBackpack = backpack.OfType<RareGem>().Count();
            rareGemCountText.text = $"Gemas Raras en Mochila: {rareInBackpack}";
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

        // ==> ACÁ ESTÁ EL AGGREGGATE <==
        var depositSummary = depositedGems.Aggregate(
            (Count: 0, TotalValue: 0, TotalWeight: 0f),
            (acc, gem) => (
                acc.Count + 1,
                acc.TotalValue + gem.Value,
                acc.TotalWeight + gem.Weight
            )
        );

        if (totalValueText != null)
            totalValueText.text = $"Valor depositado: {depositSummary.TotalValue}";

        if (rareGemDepositedText != null)
            rareGemDepositedText.text = $"Gemas Raras depositadas: { depositedGems.OfType<RareGem>().Count() }";

        // Session Stats UI
        if (totalCollectedText != null)
            totalCollectedText.text = $"Total recogidas: {sessionStats.TotalCollectedCount}";

        if (totalDepositedText != null)
            totalDepositedText.text = $"Total depositadas: {sessionStats.TotalDepositedCount}";

        if (avgValueText != null)
            avgValueText.text = $"Valor medio recogido: {sessionStats.AverageCollectedValue:F1}";

        if (maxWeightTextUI != null)
            maxWeightTextUI.text = $"Peso máximo recogido: {sessionStats.MaxCollectedWeight:F1}";

        if (mostFreqCollectedText != null)
            mostFreqCollectedText.text = $"Tipo más frecuente (recogidas): {sessionStats.MostFrequentCollectedType}";

        if (mostFreqDepositedText != null)
            mostFreqDepositedText.text = $"Tipo más frecuente (depositadas): {sessionStats.MostFrequentDepositedType}";
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gem"))
        {
            var gem = other.GetComponent<Gem>();
            if (gem == null) return;

            float currentWeight = backpack.Sum(g => g.Weight);
            if (currentWeight + gem.Weight <= maxWeight)
            {
                backpack.Add(gem);
                sessionStats.RegisterCollected(gem);
                ProcessGemCollection(gem);
                gem.Deactivate();
                gemPool?.SpawnGem();
                UpdateUI();
            }
            else if (warningText != null)
            {
                warningText.text = "¡Mochila llena! Peso máximo alcanzado.";
                warningTimer = warningDisplayTime;
            }
        }
    }

    private void ProcessGemCollection(Gem gem)
    {
        var collectionAction = new
        {
            Event     = "Collected",
            GemType   = gem.Type,
            Value     = gem.Value,
            Weight    = gem.Weight,
            Timestamp = Time.time
        };

        if (lastActionText != null)
            lastActionText.text = $"{collectionAction.Event}: {collectionAction.GemType} (Valor: {collectionAction.Value})";
    }

    public void DepositGems()
    {
        if (backpack.Count > 0)
        {
            sessionStats.RegisterDeposited(backpack);
            depositedGems.AddRange(backpack);
            backpack.Clear();

            if (warningText != null)
            {
                warningText.text = "Gemas depositadas correctamente.";
                warningTimer = warningDisplayTime;
            }

            UpdateUI();
        }
        else if (warningText != null)
        {
            warningText.text = "No hay gemas para depositar.";
            warningTimer = warningDisplayTime;
        }
    }

    public void DepositTopValuableGems(int count = 3)
    {
        var topGems = GetOrderedGemsByValue().Take(count).ToList();
        sessionStats.RegisterDeposited(topGems);
        backpack.RemoveAll(g => topGems.Contains(g));
        depositedGems.AddRange(topGems);

        if (warningText != null)
        {
            warningText.text = $"Depositaste tus {topGems.Count} gemas más valiosas.";
            warningTimer = warningDisplayTime;
        }

        UpdateUI();
    }

    private Dictionary<string, (int TotalValue, float TotalWeight, int Count)> GetGemStats()
        => backpack
            .GroupBy(g => g.Type)
            .ToDictionary(
                grp => grp.Key,
                grp => (
                    TotalValue: grp.Sum(g => g.Value),
                    TotalWeight: grp.Sum(g => g.Weight),
                    Count: grp.Count()
                )
            );

    private IEnumerable<Gem> GetHighValueGems()
    {
        foreach (var gem in backpack.Where(g => g.Value > 30))
            yield return gem;
    }

    private List<string> GetGemTypes()
        => backpack.Select(g => g.Type).Distinct().ToList();

    private IEnumerable<Gem> GetOrderedGemsByValue()
    {
        var ordered = backpack.OrderByDescending(g => g.Value).ToList();
        for (int i = 0; i < ordered.Count; i++)
            if (Time.frameCount % 2 == 0)
                yield return ordered[i];
    }
}
