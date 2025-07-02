// GemPool.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemPool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private GameObject rareGemPrefab;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 20;
    [Range(0f,1f)] [SerializeField] private float rareChance = 0.1f;
    [SerializeField] private Vector3 spawnAreaMin = new Vector3(-10f,0.5f,-10f);
    [SerializeField] private Vector3 spawnAreaMax = new Vector3( 10f,0.5f, 10f);

    private List<GameObject> gemPool = new List<GameObject>();
    private List<object> spawnHistory = new List<object>();

    void Start()
    {
        if (gemPrefab == null)
        {
            Debug.LogError("[GemPool] gemPrefab no asignado.");
            enabled = false;
            return;
        }

        // Crear pool de normales y parentarlas a este transform
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(gemPrefab, transform);
            go.SetActive(false);
            gemPool.Add(go);
        }

        StartCoroutine(SpawnWithDelay(Mathf.Min(10, poolSize), 0.05f));
    }

    public void SpawnGem()
    {
        bool makeRare = rareGemPrefab != null && Random.value < rareChance;
        GameObject go;

        if (makeRare)
        {
            go = Instantiate(rareGemPrefab, transform);
        }
        else
        {
            go = GetFromPool();
        }

        InitializeSpawn(go, makeRare);
    }

    private GameObject GetFromPool()
    {
        var go = gemPool.FirstOrDefault(g => !g.activeInHierarchy);
        if (go == null)
        {
            go = Instantiate(gemPrefab, transform);
            go.SetActive(false);
            gemPool.Add(go);
        }
        return go;
    }

    private void InitializeSpawn(GameObject go, bool isRare)
    {
        var gemScript = go.GetComponent<Gem>();
        if (gemScript != null)
        {
            string type   = isRare
                ? "Rare"
                : Random.Range(0,3) == 0 ? "Red"
                    : Random.Range(0,2) == 0 ? "Blue"
                        : "Green";

            int    value  = isRare
                ? Random.Range(50, 100)
                : Random.Range(10, 50);

            float  weight = isRare
                ? Random.Range(5f, 15f)
                : Random.Range(1f, 10f);

            gemScript.Initialize(type, value, weight);

            spawnHistory.Add(new {
                Event     = isRare ? "SpawnRare" : "Spawn",
                Type      = type,     // CORRECCIÓN: variable local 'type'
                Value     = value,
                Weight    = weight,
                Timestamp = Time.time
            });
        }

        go.transform.position = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            spawnAreaMin.y,
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );
        go.SetActive(true);
    }

    public IEnumerator SpawnWithDelay(int count, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnGem();
            yield return new WaitForSeconds(delay);
        }
    }
}
