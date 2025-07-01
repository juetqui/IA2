using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gestiona un pool de objetos para gemas, permitiendo reutilizar instancias en lugar de instanciar/destruir.
/// Genera gemas en posiciones aleatorias dentro de un área definida.
/// </summary>
public class GemPool : MonoBehaviour
{
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private Vector3 spawnAreaMin = new Vector3(-10f, 0.5f, -10f);
    [SerializeField] private Vector3 spawnAreaMax = new Vector3(10f, 0.5f, 10f);

    private List<GameObject> gemPool = new List<GameObject>();

    /// <summary>
    /// Inicializa el pool de gemas y genera las gemas iniciales.
    /// </summary>
    void Start()
    {
        if (gemPrefab == null)
        {
            return;
        }

        gemPool = Enumerable.Range(0, poolSize)
            .Select(_ =>
            {
                var gem = Instantiate(gemPrefab, Vector3.zero, Quaternion.identity);
                gem.SetActive(false);
                return gem;
            })
            .ToList();

        SpawnInitialGems();
    }

    /// <summary>
    /// Genera un número inicial de gemas en la escena.
    /// </summary>
    private void SpawnInitialGems()
    {
        int gemsToSpawn = Mathf.Min(10, poolSize);
        for (int i = 0; i < gemsToSpawn; i++)
        {
            SpawnGem();
        }
    }

    /// <summary>
    /// Obtiene una gema inactiva del pool o crea una nueva si no hay disponibles.
    /// Inicializa y activa la gema.
    /// </summary>
    /// <returns>La gema obtenida o creada.</returns>
    public GameObject GetGem()
    {
        var gem = gemPool.FirstOrDefault(g => !g.activeInHierarchy);

        if (gem == null)
        {
            gem = Instantiate(gemPrefab, Vector3.zero, Quaternion.identity);
            gem.SetActive(false);
            gemPool.Add(gem);
        }

        InitializeGem(gem);
        return gem;
    }

    /// <summary>
    /// Inicializa una gema con propiedades aleatorias (tipo, valor, peso) y la posiciona en un lugar aleatorio.
    /// </summary>
    /// <param name="gem">La gema a inicializar.</param>
    private void InitializeGem(GameObject gem)
    {
        Gem gemScript = gem.GetComponent<Gem>();
        if (gemScript != null)
        {
            string type = Random.Range(0, 3) == 0 ? "Red" : Random.Range(0, 2) == 0 ? "Blue" : "Green";
            int value = Random.Range(10, 50);
            float weight = Random.Range(1f, 10f);
            gemScript.Initialize(type, value, weight);
        }

        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            spawnAreaMin.y,
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );
        gem.transform.position = spawnPosition;

        gem.SetActive(true);
    }

    /// <summary>
    /// Genera una nueva gema en la escena reutilizando una del pool.
    /// </summary>
    public void SpawnGem()
    {
        GameObject gem = GetGem();
        gem.SetActive(true);
    }
}