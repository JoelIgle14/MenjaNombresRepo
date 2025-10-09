using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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

    public enum OperationType
    {
        BasicClient,
        SophisticatedCustomer,
        SequentialCustomer,
        PickyCustomer,
        IntellectualCustomer,
        MultiplierCustomer
    }

    [System.Serializable]
    public class MonsterType
    {
        public string name;
        public GameObject prefab;
        public int points;
        public float weight;
        public OperationType operationType;
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
            List<MonsterType> availableTypes = new List<MonsterType>(monsterTypes);

            for (int i = 0; i < maxMonsters && i < spawnPoints.Length; i++)
            {
                if (availableTypes.Count == 0)
                    break;

                Transform spawn = spawnPoints[i];
                MonsterType chosenType = GetRandomMonsterType(availableTypes);
                availableTypes.Remove(chosenType);

                GameObject monster = Instantiate(chosenType.prefab, spawn.position, Quaternion.identity);

                // Configuración del movimiento
                var risingScript = monster.GetComponent<RisingMonster>();
                if (risingScript != null)
                {
                    risingScript.targetLine = targetLine;
                    risingScript.speed = monsterSpeed;
                    risingScript.stayTime = monsterStayTime;
                    risingScript.startY = monsterStartY;
                }

                // Generar una operación válida
                string operation = GenerateValidOperation(chosenType.operationType);

                // Mostrar visualmente con TextMeshPro
                TextMeshPro tmp = monster.GetComponentInChildren<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = operation;
                }

                Debug.Log($"Spawned: {chosenType.name} ({chosenType.points} pts) | Operation: {operation}");

                currentWave.Add(monster);
            }

            while (currentWave.Exists(m => m != null))
                yield return null;

            yield return new WaitForSeconds(spawnDelay);
        }
    }

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

        return pool[0];
    }

    // ---- Nuevo: genera operación y la valida ----
    string GenerateValidOperation(OperationType type)
    {
        string op;
        int result;

        int safety = 0; // evita bucle infinito por error
        do
        {
            op = GenerateOperation(type, out result);
            safety++;
            if (safety > 50) break;
        }
        while (result < 1 || result > 9);

        return op;
    }

    string GenerateOperation(OperationType type, out int result)
    {
        result = -1;

        switch (type)
        {
            case OperationType.BasicClient:
                int exact = Random.Range(1, 10);
                result = exact;
                return $"Wants {exact}";

            case OperationType.SophisticatedCustomer:
                if (Random.value < 0.5f)
                {
                    int a = Random.Range(1, 9);
                    int b = Random.Range(1, 9);
                    result = a + b;
                    return $"{a} + {b} = ?";
                }
                else
                {
                    int x = Random.Range(1, 9);
                    int res = Random.Range(1, 9);
                    result = res - x;
                    return $"{res} - X = {x}";
                }

            case OperationType.SequentialCustomer:
                int n1 = Random.Range(1, 10);
                int n2 = Random.Range(1, 10);
                int n3 = Random.Range(1, 10);
                result = n3; // último valor
                return $"{n1}-{n2}-{n3}";

            case OperationType.PickyCustomer:
                bool even = Random.value < 0.5f;
                result = even ? 2 : 1;
                return even ? "Even only" : "Odd only";

            case OperationType.IntellectualCustomer:
                int min = Random.Range(1, 5);
                int max = Random.Range(6, 10);
                result = Random.Range(min, max);
                string parity = (result % 2 == 0) ? "even" : "odd";
                return $"Find {parity} between {min} and {max}";

            case OperationType.MultiplierCustomer:
                if (Random.value < 0.5f)
                {
                    int a2 = Random.Range(1, 10);
                    int b2 = Random.Range(1, 10);
                    result = a2 * b2;
                    return $"{a2} × {b2} = ?";
                }
                else
                {
                    int divisor = Random.Range(1, 9);
                    int quotient = Random.Range(1, 9);
                    result = divisor * quotient;
                    return $"{result} ÷ {divisor} = ?";
                }

            default:
                return "Unknown request";
        }
    }
}
