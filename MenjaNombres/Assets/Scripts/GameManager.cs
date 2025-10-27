using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

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
    public TMP_Text Score;

    // ==== SPECIAL EFFECT SYSTEM ====
    [Header("Special Effects (Runtime)")]
    private bool blueEffectActive = false;
    private bool yellowEffectActive = false;
    private float yellowMultiplier = 1f;
    private Coroutine yellowRoutine;
    private Coroutine blueRoutine;
    public CameraEffects effects;


    public float gameTime = 0f;  // Tiempo de juego en segundos
    public int completedOrders = 0;  // Pedidos completados

    PlAud aud;

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

    public enum BoxEffectType
    {
        BlueEffect,
        YellowEffect,
        GreenEffect,
        PurpleEffect
    }

    [System.Serializable]
    public class BoxType
    {
        public string name;
        public GameObject prefab;
        public BoxEffectType effectType;
    }

    [Header("Box Configuration")]
    public List<BoxType> boxTypes = new List<BoxType>();
    public float specialBoxInterval = 20f; // Cada 20 segundos aparece una caja
    private float nextBoxTime = 0f;


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {

        aud = GetComponent<PlAud>();
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
        nextBoxTime = specialBoxInterval; // primer spawn de caja a los 20 segundos

    }

    void Update()
    {
        if (gameActive)
        {
            gameTime += Time.deltaTime;  

        }
        if (gameActive && gameTime >= nextBoxTime)
        {
            SpawnSpecialBoxOnRandomBelt();
            nextBoxTime += specialBoxInterval;
        }

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
            //belt.GetComponent<SpriteRenderer>().material.GetFloat("");
            belt.GetComponent<SpriteRenderer>().material.SetFloat("_Speed", belt.moveSpeed * ((!belt.moveRight) ?  0.1f : -0.1f));
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
                return $"Vull menjar-me el nombre {result}";

            case OperationType.SophisticatedCustomer:
                if (Random.value < 0.5f)
                {
                    int a = Random.Range(1, max / 2);
                    int b = Random.Range(1, max / 2);
                    result = a + b;
                    return $"Vull menjar-me\n {a} + {b} = ?";
                }
                else
                {
                    int a = Random.Range(1, max / 2);
                    int b = Random.Range(1, max / 2);

                    // Evitar resultados negativos
                    if (b > a)
                    {
                        int temp = a;
                        a = b;
                        b = temp;
                    }

                    result = a - b;
                    return $"Vull menjar-me\n {a} - {b} = ?";
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
                return even ? "Per menjar només vull nombres parells" : "Només vull nombres imparells";

            case OperationType.IntellectualCustomer:
                int rangeMin = Random.Range(min, max - 2);
                int rangeMax = Random.Range(rangeMin + 2, max + 1);
                result = Random.Range(rangeMin, rangeMax + 1);
                string parity = (result % 2 == 0) ? "parell" : "imparell";
                return $"Vull un nombre {parity} entre {rangeMin} i {rangeMax} per menjar!";

            case OperationType.MultiplierCustomer:
                if (Random.value < 0.5f)
                {
                    int a = Random.Range(2, Mathf.Clamp(max / 3, 3, max));
                    int b = Random.Range(2, Mathf.Clamp(max / 3, 3, max));
                    result = a * b;
                    return $"Porta’m una ració de {a} multiplicat {b}";
                }
                else
                {
                    int divisor = Random.Range(2, Mathf.Clamp(max / 3, 3, max));
                    int quotient = Random.Range(2, Mathf.Clamp(max / divisor, 3, max));
                    int dividend = divisor * quotient; // número que se mostrará
                    result = quotient; // el resultado correcto de la operación
                    return $"Porta’m una ració de {dividend} dividit {divisor}";
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
        completedOrders++;  
        Score.text = "Puntuació: " + score;
    }

    public void OnMonsterFailed()
    {
        if (lives > 0)
        {
            aud.PlayAud();
            lives--;
            effects.Damage();
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

    void SpawnSpecialBoxOnRandomBelt()
    {
        ConveyorBelt[] belts = FindObjectsOfType<ConveyorBelt>();
        if (belts.Length == 0 || boxTypes.Count == 0) return;

        // Elegir una cinta aleatoria
        ConveyorBelt randomBelt = belts[Random.Range(0, belts.Length)];

        // Elegir un tipo de caja aleatorio
        BoxType chosenBox = boxTypes[Random.Range(0, boxTypes.Count)];

        // Pedirle a la cinta que genere una caja especial
        randomBelt.QueueBoxSpawn(chosenBox.prefab, chosenBox.effectType);

        Debug.Log($"[GameManager] Spawning special box: {chosenBox.name} ({chosenBox.effectType})");
    }

    public void ActivateBoxEffect(BoxEffectType effectType)
    {
        switch (effectType)
        {
            case BoxEffectType.BlueEffect:
                if (blueRoutine != null) StopCoroutine(blueRoutine);
                blueRoutine = StartCoroutine(BluePauseEffect());
                break;

            case BoxEffectType.YellowEffect:
                if (yellowRoutine != null) StopCoroutine(yellowRoutine);
                yellowRoutine = StartCoroutine(YellowDoublePoints());
                break;

            case BoxEffectType.GreenEffect:
                ActivateGreenEffect();
                break;

            case BoxEffectType.PurpleEffect:
                ActivatePurpleEffect();
                break;
        }
    }

    private IEnumerator BluePauseEffect()
    {
        blueEffectActive = true;
        Debug.Log(" Blue Effect activated! Pausing all monsters for 10s");

        // Pausar todos los timers
        Monster[] monsters = FindObjectsOfType<Monster>();
        foreach (var m in monsters)
            m.PauseTimer(true);

        yield return new WaitForSeconds(10f);

        // Reanudar timers
        foreach (var m in monsters)
            m.PauseTimer(false);

        blueEffectActive = false;
        Debug.Log(" Blue Effect ended!");
    }

    private IEnumerator YellowDoublePoints()
    {
        yellowEffectActive = true;
        yellowMultiplier = 2f;
        Debug.Log(" Yellow Effect activated! Double points for 30s");

        yield return new WaitForSeconds(30f);

        yellowMultiplier = 1f;
        yellowEffectActive = false;
        Debug.Log(" Yellow Effect ended!");
    }

    private void ActivateGreenEffect()
    {
        Debug.Log(" Green Effect activated! More numbers spawning temporarily.");

        ConveyorBelt[] belts = FindObjectsOfType<ConveyorBelt>();
        foreach (var belt in belts)
        {
            belt.StartCoroutine(ExtraNumbersBurst(belt));
        }
    }

    private IEnumerator ExtraNumbersBurst(ConveyorBelt belt)
    {
        float originalSpeed = belt.moveSpeed;
        belt.moveSpeed *= 1.5f;

        // Hacer que genere números adicionales temporalmente
        for (int i = 0; i < 4; i++)
        {
            belt.QueueNumberSpawn(UnityEngine.Random.Range(1, 9));
            yield return new WaitForSeconds(2f);
        }

        belt.moveSpeed = originalSpeed;
    }

    private void ActivatePurpleEffect()
    {
        Debug.Log("Purple Effect activated! Completing all orders instantly.");

        Monster[] monsters = FindObjectsOfType<Monster>();
        foreach (var m in monsters)
        {
            m.CompleteInstantly();
        }
    }

    // Este método se llama por Monster al sumar puntos
    public int ApplyScoreMultiplier(int basePoints)
    {
        return Mathf.RoundToInt(basePoints * yellowMultiplier);
    }



    void GameOver()
    {
        gameActive = false;
        Debug.Log($"Game Over! Final Score: {score}");

        // Cambiar a la escena de resultados
        SceneManager.LoadScene("GameOverScene");

        // Para pasar los datos a la nueva escena (usamos PlayerPrefs o un Singleton/ScriptableObject)
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.SetFloat("FinalGameTime", gameTime);
        PlayerPrefs.SetInt("FinalCompletedOrders", completedOrders);
    }

}