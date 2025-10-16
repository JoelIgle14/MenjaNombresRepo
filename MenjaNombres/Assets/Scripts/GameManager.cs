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

    [Header("Monster Configuration")]
    public float monsterSpeed = 3f;
    public float monsterStayTime = 5f;
    public float monsterStartY = -10f;

    [Header("Difficulty Settings")]
    public DifficultyLevel[] difficultyLevels;
    [HideInInspector]
    public int currentDifficultyIndex = 0;

    [Header("Game State")]
    public int lives = 3;
    public int score = 0;
    private bool gameActive = true;
    public GameObject[] lifeObjects;

    public static GameManager Instance;

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
        public float baseWeight;
        public OperationType operationType;

        [HideInInspector] public float currentWeight;
    }

    [System.Serializable]
    public class DifficultyLevel
    {
        public string levelName = "Easy";
        public int clientNum = 1;
        public float conveyorSpeedMultiplier = 1f;
        public int minNumValue = 1;
        public int maxNumValue = 5;
        public float orderTimeMultiplier = 1f;
        public float durationSeconds = 40f;

        [Header("Monster Weight Multipliers")]
        public float basicClientWeight = 1f;
        public float sophisticatedWeight = 0.5f;
        public float sequentialWeight = 0.3f;
        public float pickyWeight = 0.3f;
        public float intellectualWeight = 0.1f;
        public float multiplierWeight = 0.1f;
    }

    [Header("Monster Types")]
    public List<MonsterType> monsterTypes = new List<MonsterType>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (monsterTypes.Count == 0)
        {
            Debug.LogError("No monster types assigned in GameManager.");
            return;
        }

        if (difficultyLevels.Length == 0)
        {
            Debug.LogError("No difficulty levels assigned in GameManager.");
            return;
        }

        ApplyDifficulty(0);
        StartCoroutine(SpawnWaveLoop());
        StartCoroutine(DifficultyProgression());
    }

    IEnumerator DifficultyProgression()
    {
        while (gameActive && currentDifficultyIndex < difficultyLevels.Length)
        {
            DifficultyLevel current = difficultyLevels[currentDifficultyIndex];

            // Wait for difficulty duration (unless it's the final difficulty)
            if (currentDifficultyIndex < difficultyLevels.Length - 1)
            {
                yield return new WaitForSeconds(current.durationSeconds);
                currentDifficultyIndex++;
                ApplyDifficulty(currentDifficultyIndex);
                Debug.Log($"Difficulty increased to: {difficultyLevels[currentDifficultyIndex].levelName}");
            }
            else
            {
                // Final difficulty - stay here until game over
                yield break;
            }
        }
    }

    void ApplyDifficulty(int index)
    {
        if (index >= difficultyLevels.Length) return;

        DifficultyLevel level = difficultyLevels[index];
        maxMonsters = level.clientNum;

        // Update monster weights based on difficulty
        foreach (MonsterType monster in monsterTypes)
        {
            float weightMultiplier = 1f;

            switch (monster.operationType)
            {
                case OperationType.BasicClient:
                    weightMultiplier = level.basicClientWeight;
                    break;
                case OperationType.SophisticatedCustomer:
                    weightMultiplier = level.sophisticatedWeight;
                    break;
                case OperationType.SequentialCustomer:
                    weightMultiplier = level.sequentialWeight;
                    break;
                case OperationType.PickyCustomer:
                    weightMultiplier = level.pickyWeight;
                    break;
                case OperationType.IntellectualCustomer:
                    weightMultiplier = level.intellectualWeight;
                    break;
                case OperationType.MultiplierCustomer:
                    weightMultiplier = level.multiplierWeight;
                    break;
            }

            monster.currentWeight = monster.baseWeight * weightMultiplier;
        }

        // Notify conveyor belts about speed change
        ConveyorBelt[] belts = FindObjectsOfType<ConveyorBelt>();
        foreach (ConveyorBelt belt in belts)
        {
            belt.moveSpeed *= level.conveyorSpeedMultiplier;
        }
    }

    IEnumerator SpawnWaveLoop()
    {
        while (gameActive)
        {
            List<GameObject> currentWave = new List<GameObject>();
            List<MonsterType> availableTypes = new List<MonsterType>(monsterTypes);

            int monstersToSpawn = Mathf.Min(maxMonsters, spawnPoints.Length);

            for (int i = 0; i < monstersToSpawn; i++)
            {
                if (availableTypes.Count == 0)
                    break;

                Transform spawn = spawnPoints[i];
                MonsterType chosenType = GetRandomMonsterType(availableTypes);
                availableTypes.Remove(chosenType);

                GameObject monster = Instantiate(chosenType.prefab, spawn.position, Quaternion.identity);

                // Configure movement
                var risingScript = monster.GetComponent<RisingMonster>();
                if (risingScript != null)
                {
                    risingScript.targetLine = targetLine;
                    risingScript.speed = monsterSpeed;
                    risingScript.stayTime = monsterStayTime * GetCurrentDifficulty().orderTimeMultiplier;
                    risingScript.startY = monsterStartY;

                    risingScript.InitializeRising(targetLine);

                }

                // Configure monster data
                var monsterScript = monster.GetComponent<Monster>();
                if (monsterScript != null)
                {
                    int result;
                    string operation = GenerateValidOperation(chosenType.operationType, out result);
                    monsterScript.Initialize(operation, result, chosenType.points, chosenType.operationType);
                }

                Debug.Log($"Spawned: {chosenType.name} ({chosenType.points} pts)");

                currentWave.Add(monster);
            }

            // Wait for all monsters in wave to be destroyed
            while (currentWave.Exists(m => m != null))
                yield return null;

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    MonsterType GetRandomMonsterType(List<MonsterType> pool)
    {
        float totalWeight = 0f;
        foreach (MonsterType t in pool)
            totalWeight += t.currentWeight;

        float random = Random.Range(0, totalWeight);
        float cumulative = 0f;

        foreach (MonsterType t in pool)
        {
            cumulative += t.currentWeight;
            if (random <= cumulative)
                return t;
        }

        return pool[0];
    }

    string GenerateValidOperation(OperationType type, out int result)
    {
        string op;
        result = -1;
        DifficultyLevel difficulty = GetCurrentDifficulty();

        int safety = 0;
        do
        {
            op = GenerateOperation(type, difficulty, out result);
            safety++;
            if (safety > 100) break;
        }
        while (result < difficulty.minNumValue || result > difficulty.maxNumValue);

        return op;
    }

    string GenerateOperation(OperationType type, DifficultyLevel difficulty, out int result)
    {
        result = -1;
        int min = difficulty.minNumValue;
        int max = difficulty.maxNumValue;

        switch (type)
        {
            case OperationType.BasicClient:
                result = Random.Range(min, max + 1);
                return $"Wants {result}";

            case OperationType.SophisticatedCustomer:
                if (Random.value < 0.5f)
                {
                    int a = Random.Range(1, max / 2);
                    int b = Random.Range(1, max / 2);
                    result = a + b;
                    return $"{a} + {b} = ?";
                }
                else
                {
                    int res = Random.Range(min + 2, max + 1);
                    int x = Random.Range(1, res);
                    result = res - x;
                    return $"{res} - X = {x}";
                }

            case OperationType.SequentialCustomer:
                int n1 = Random.Range(min, max + 1);
                int n2 = Random.Range(min, max + 1);
                int n3 = Random.Range(min, max + 1);
                result = n3;
                return $"{n1}-{n2}-{n3}";

            case OperationType.PickyCustomer:
                bool even = Random.value < 0.5f;
                result = Random.Range(min, max + 1);
                while ((result % 2 == 0) != even)
                    result = Random.Range(min, max + 1);
                return even ? "Even only" : "Odd only";

            case OperationType.IntellectualCustomer:
                int rangeMin = Random.Range(min, max - 2);
                int rangeMax = Random.Range(rangeMin + 2, max + 1);
                result = Random.Range(rangeMin, rangeMax + 1);
                string parity = (result % 2 == 0) ? "even" : "odd";
                return $"Find {parity} between {rangeMin} and {rangeMax}";

            case OperationType.MultiplierCustomer:
                if (Random.value < 0.5f)
                {
                    int a = Random.Range(2, Mathf.Min(5, max / 2));
                    int b = Random.Range(2, Mathf.Min(5, max / 2));
                    result = a * b;
                    return $"{a} × {b} = ?";
                }
                else
                {
                    int divisor = Random.Range(2, Mathf.Min(5, max / 2));
                    int quotient = Random.Range(2, max / divisor);
                    result = divisor * quotient;
                    return $"{result} ÷ {divisor} = ?";
                }

            default:
                result = Random.Range(min, max + 1);
                return "Unknown request";
        }
    }

    DifficultyLevel GetCurrentDifficulty()
    {
        return difficultyLevels[currentDifficultyIndex];
    }

    public void OnMonsterServed(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
    }

    public void OnMonsterFailed()
    {
        if (lives > 0)
        {
            lives--;

            // Solo destruir si el índice sigue válido
            if (lives >= 0 && lives < lifeObjects.Length && lifeObjects[lives] != null)
            {
                Destroy(lifeObjects[lives]);
            }

            Debug.Log($"Lives remaining: {lives}");
        }

        if (lives <= 0)
        {
            GameOver();
        }
    }


    void GameOver()
    {
        gameActive = false;
        Debug.Log($"Game Over! Final Score: {score}");
    }
}