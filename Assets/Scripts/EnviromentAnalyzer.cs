// EnvironmentAnalyzer.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;

public class EnvironmentAnalyzer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI environmentStatsText;
    [SerializeField] private TextMeshProUGUI topGemsText;
    [SerializeField] private TextMeshProUGUI groupedGemsText;
    [SerializeField] private TextMeshProUGUI rareGemsSceneText;

    private List<Gem> gemsInScene = new List<Gem>();

    void Start()
    {
        UpdateEnvironmentUI();
    }

    /// <summary>
    /// Obtiene todas las Gem activas en la escena.
    /// </summary>
    public void UpdateGemsInScene()
    {
        gemsInScene = FindObjectsOfType<Gem>()
            .Where(g => g.gameObject.activeInHierarchy)
            .ToList();

        Debug.Log($"[Env] Gemas en escena: {gemsInScene.Count}");
        foreach (var gem in gemsInScene)
            Debug.Log($"[Env] Gema '{gem.Type}' en posición {gem.transform.position}");
    }

    /// <summary>
    /// Actualiza la UI completa.
    /// </summary>
    public void UpdateEnvironmentUI()
    {
        if (environmentStatsText == null) return;

        UpdateGemsInScene();

        int total = gemsInScene.Count;
        var values  = gemsInScene.Select(g => g.Value);
        var weights = gemsInScene.Select(g => g.Weight);

        int   minValue  = total>0?values.Min():0;
        int   maxValue  = total>0?values.Max():0;
        float avgValue  = total>0?(float)values.Average():0f;
        float minWeight = total>0?weights.Min():0f;
        float maxWeight = total>0?weights.Max():0f;
        float avgWeight = total>0?(float)weights.Average():0f;

        const int highThreshold = 40;
        int highCount = gemsInScene.Count(g=>g.Value>highThreshold);
        float pctHigh = total>0?100f*highCount/total:0f;

        int rareCount = gemsInScene.OfType<RareGem>().Count();
        if (rareGemsSceneText!=null)
            rareGemsSceneText.text = $"Gemas Raras: {rareCount}";

        var ranges = gemsInScene
            .GroupBy(g => g.Value<20?"Low":g.Value<40?"Medium":"High")
            .ToDictionary(grp=>grp.Key,grp=>grp.Count());

        var countByType = gemsInScene
            .GroupBy(g=>g.Type)
            .ToDictionary(grp=>grp.Key,grp=>grp.Count());

        (string Type,int Value,float Distance) closestInfo = ("None",0,0f);
        if (playerTransform!=null && total>0)
        {
            var closest = gemsInScene
                .OrderBy(g=>Vector3.Distance(playerTransform.position,g.transform.position))
                .First();
            float dist = Vector3.Distance(playerTransform.position,closest.transform.position);
            closestInfo = (closest.Type,closest.Value,dist);
        }

        var sb = new StringBuilder();
        sb.AppendLine($"<b>Entorno:</b> {total} gemas");
        sb.AppendLine($"Valor → Min:{minValue}, Máx:{maxValue}, Prom:{avgValue:F1}");
        sb.AppendLine($"Peso  → Min:{minWeight:F1}, Máx:{maxWeight:F1}, Prom:{avgWeight:F1}");
        sb.AppendLine($"Gemas >{highThreshold}: {highCount} ({pctHigh:F1}%)");
        sb.AppendLine($"Gemas Raras: {rareCount}");
        sb.AppendLine();
        sb.AppendLine("<b>Por rango:</b>");
        foreach(var kv in ranges) sb.AppendLine($"•{kv.Key}: {kv.Value}");
        sb.AppendLine();
        sb.AppendLine("<b>Por tipo:</b>");
        foreach(var kv in countByType) sb.AppendLine($"•{kv.Key}: {kv.Value}");
        sb.AppendLine();
        sb.AppendLine($"<b>Cercana:</b> {closestInfo.Type} (V:{closestInfo.Value}, D:{closestInfo.Distance:F1}m)");

        environmentStatsText.text = sb.ToString();

        UpdateTopGemsUI();
        if(groupedGemsText!=null) groupedGemsText.text = string.Empty;
    }

    private void UpdateTopGemsUI()
    {
        if (topGemsText==null) return;

        var top5 = gemsInScene.Take(5).ToList();
        if(top5.Count==0)
        {
            topGemsText.text = "Top 5 gemas: ninguna activa.";
            return;
        }

        var sb = new StringBuilder("<b>Top 5 gemas:</b>\n");
        foreach(var gem in top5)
        {
            string line = $"- {gem.Type} (V:{gem.Value}, W:{gem.Weight:F1})";
            if(gem is RareGem) line = $"<color=magenta>{line}</color>";
            sb.AppendLine(line);
        }
        topGemsText.text = sb.ToString();
    }

    public IEnumerator DisplayGroupedGems()
    {
        if(groupedGemsText==null) yield break;

        groupedGemsText.text = "<b>Agrupando por tipo...</b>\n";
        var byType = gemsInScene.GroupBy(g=>g.Type);
        foreach(var grp in byType)
        {
            groupedGemsText.text += $"<b>{grp.Key} ({grp.Count()}):</b>\n";
            yield return new WaitForSeconds(0.1f);
            foreach(var gem in grp.Take(3))
            {
                string line = $"    - V:{gem.Value}, W:{gem.Weight:F1}";
                if(gem is RareGem) line = $"<color=magenta>{line}</color>";
                groupedGemsText.text += line + "\n";
                yield return new WaitForSeconds(0.1f);
            }
        }
        groupedGemsText.text += "\nFin del escaneo.";
    }

    /// <summary>
    /// Dibuja siempre un gizmo para cada gema detectada.
    /// </summary>
    void OnDrawGizmos()
    {
        if (gemsInScene == null) return;
        Gizmos.color = Color.yellow;
        foreach (var gem in gemsInScene)
            Gizmos.DrawSphere(gem.transform.position, 0.2f);
    }
}
