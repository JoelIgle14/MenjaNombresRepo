using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Spawn")]
    public Transform[] spawnPoints;
    public Transform targetLine;
    public float spawnDelay = 2f;
    public int maxMonsters = 3;

    [Header("Configuración de monstruos")]
    public float monsterSpeed = 2f;
    public float monsterStayTime = 3f;
    public float monsterStartY = -10f;

    private bool gameActive = true;

    // ---- Tipos de monstruos ----
    [System.Serializable]
    public class MonsterType
    {
        public string name;
        public GameObject prefab;
        public int points;
        public float weight; // más alto = más probable
    }

    [Header("Prefabs y probabilidades")]
    public List<MonsterType> monsterTypes = new List<MonsterType>();

    void Start()
    {
        if (monsterTypes.Count == 0)
        {
            Debug.LogError("No hay tipos de monstruos asignados en el GameManager.");
            return;
        }

        StartCoroutine(SpawnWaveLoop());
    }

    IEnumerator SpawnWaveLoop()
    {
        while (gameActive)
        {
            List<GameObject> currentWave = new List<GameObject>();

            // --- Crear una copia temporal de tipos de monstruos ---
            List<MonsterType> availableTypes = new List<MonsterType>(monsterTypes);

            for (int i = 0; i < maxMonsters && i < spawnPoints.Length; i++)
            {
                if (availableTypes.Count == 0)
                    break; // por si hay menos tipos que maxMonsters

                Transform spawn = spawnPoints[i];

                // Elegir tipo de monstruo aleatorio (ponderado, sin repetición)
                MonsterType chosenType = GetRandomMonsterType(availableTypes);

                // Eliminarlo de la lista para no repetirlo esta oleada
                availableTypes.Remove(chosenType);

                // Instanciar su prefab
                GameObject monster = Instantiate(chosenType.prefab, spawn.position, Quaternion.identity);

                // Configurar su script
                var risingScript = monster.GetComponent<RisingMonster>();
                if (risingScript != null)
                {
                    risingScript.targetLine = targetLine;
                    risingScript.speed = monsterSpeed;
                    risingScript.stayTime = monsterStayTime;
                    risingScript.startY = monsterStartY;
                }

                Debug.Log($"Spawned: {chosenType.name} ({chosenType.points} pts)");

                currentWave.Add(monster);
            }

            // Esperar hasta que todos se destruyan antes de siguiente oleada
            while (currentWave.Exists(m => m != null))
                yield return null;

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    // ---- Selección ponderada ----
    MonsterType GetRandomMonsterType(List<MonsterType> pool)
    {
        float totalWeight = 0f;
        foreach (var t in pool)
            totalWeight += t.weight;

        float random = Random.Range(0, totalWeight);
        float cumulative = 0f;

        foreach (var t in pool)
        {
            cumulative += t.weight;
            if (random <= cumulative)
                return t;
        }

        return pool[0]; // fallback
    }

}
