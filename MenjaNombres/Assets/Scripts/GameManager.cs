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

    [Header("Tutorial")]
    public bool isTutorialMode = false;

    public float gameTime = 0f;
    public int completedOrders = 0;


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
    public float specialBoxInterval = 20f;
    private float nextBoxTime = 0f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        gameActive = true;
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


        // APLICA LA DIFICULTAD ANTES DE INICIAR LOS CICLOS
        ApplyDifficulty(0);

        if (!isTutorialMode)
            StartCoroutine(SpawnWaveLoop());

        StartCoroutine(DifficultyProgression());

        nextBoxTime = specialBoxInterval;
    }


    void Update()
    {
        if (gameActive)
        {
            print(Time.timeScale);
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

            if (currentDifficultyIndex < difficultyLevels.Length - 1)
            {
                yield return new WaitForSeconds(current.durationSeconds);
                currentDifficultyIndex++;
                ApplyDifficulty(currentDifficultyIndex);
                Debug.Log($"Difficulty increased to: {difficultyLevels[currentDifficultyIndex].levelName}");
            }
            else
            {
                yield break;
            }
        }
    }

    void ApplyDifficulty(int index)
    {
        if (index >= difficultyLevels.Length) return;

        DifficultyLevel level = difficultyLevels[index];
        maxMonsters = level.clientNum;

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

        ConveyorBelt[] belts = FindObjectsOfType<ConveyorBelt>();
        foreach (ConveyorBelt belt in belts)
        {
            belt.moveSpeed *= level.conveyorSpeedMultiplier;
            belt.GetComponent<SpriteRenderer>().material.SetFloat("_Speed", belt.moveSpeed * ((!belt.moveRight) ? 0.1f : -0.1f));
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

                var risingScript = monster.GetComponent<RisingMonster>();
                if (risingScript != null)
                {
                    risingScript.targetLine = targetLine;
                    risingScript.speed = monsterSpeed;
                    risingScript.stayTime = monsterStayTime * GetCurrentDifficulty().orderTimeMultiplier;
                    risingScript.startY = monsterStartY;
                    risingScript.InitializeRising(targetLine);
                }

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
                    return $"Porta'm una ració de {a} multiplicat {b}";
                }
                else
                {
                    int divisor = Random.Range(2, Mathf.Clamp(max / 3, 3, max));
                    int quotient = Random.Range(2, Mathf.Clamp(max / divisor, 3, max));
                    int dividend = divisor * quotient;
                    result = quotient;
                    return $"Porta'm una ració de {dividend} dividit {divisor}";
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

        // Notify tutorial if active
        if (isTutorialMode && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnNumberDroppedToMonster();
        }
    }

    public void OnMonsterFailed()
    {
        if (lives > 0)
        {
            aud.PlayAud();
            lives--;
            effects.Damage();

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

        ConveyorBelt randomBelt = belts[Random.Range(0, belts.Length)];
        BoxType chosenBox = boxTypes[Random.Range(0, boxTypes.Count)];
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

        Monster[] monsters = FindObjectsOfType<Monster>();
        foreach (var m in monsters)
            m.PauseTimer(true);

        yield return new WaitForSeconds(10f);

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
        if (isTutorialMode) FindAnyObjectByType<TutorialManager>().OnSpecialBoxUsed();

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

    public int ApplyScoreMultiplier(int basePoints)
    {
        return Mathf.RoundToInt(basePoints * yellowMultiplier);
    }

    public void ForceBasicCustomerPhase()
    {
        // 1. Remove all monsters
        RemoveAllMonsters();

        // 2. Spawn basic monster requesting 1
        MonsterType basicType = monsterTypes.Find(m => m.operationType == OperationType.BasicClient);
        if (basicType != null && spawnPoints.Length > 0)
        {
            GameObject monster = Instantiate(basicType.prefab, spawnPoints[0].position, Quaternion.identity);
            var monsterScript = monster.GetComponent<Monster>();
            var risingScript = monster.GetComponent<RisingMonster>();
            if (risingScript != null)
            {
                risingScript.targetLine = targetLine;
                risingScript.speed = monsterSpeed * 0.5f; // Slower for tutorial
                risingScript.stayTime = monsterStayTime * 3f; // Much longer time
                risingScript.startY = monsterStartY;
                risingScript.InitializeRising(targetLine);
            }
            if (monsterScript != null)
            {
                int result = 1;
                monsterScript.Initialize($"Vull menjar-me el nombre {result}", result, basicType.points, basicType.operationType);
                monsterScript.orderTime = float.PositiveInfinity;
            }
        }

        // 3. Force all belts to have only 1s and 2s
        foreach (var belt in FindObjectsOfType<ConveyorBelt>())
        {
            belt.isTutorialMode = true;
            belt.SetTutorialNumbers(new int[] { 1, 2, 1, 2 });
        }
    }

    public void ForceAdditionPhase()
    {
        RemoveAllMonsters();

        MonsterType addType = monsterTypes.Find(m => m.operationType == OperationType.SophisticatedCustomer);
        if (addType != null && spawnPoints.Length > 0)
        {
            GameObject monster = Instantiate(addType.prefab, spawnPoints[0].position, Quaternion.identity);
            var monsterScript = monster.GetComponent<Monster>();
            var risingScript = monster.GetComponent<RisingMonster>();
            if (risingScript != null)
            {
                risingScript.targetLine = targetLine;
                risingScript.speed = monsterSpeed * 0.5f; // Slower for tutorial
                risingScript.stayTime = monsterStayTime * 3f; // Much longer time
                risingScript.startY = monsterStartY;
                risingScript.InitializeRising(targetLine);
            }
            if (monsterScript != null)
            {
                int a = 2;
                int b = 3;
                int result = a + b;
                monsterScript.Initialize($"Vull menjar-me\n {a} + {b} = ?", result, addType.points, addType.operationType);
                monsterScript.orderTime = float.PositiveInfinity;
            }
        }

        foreach (var belt in FindObjectsOfType<ConveyorBelt>())
        {
            belt.ClearTutorialNumbers();
            belt.isTutorialMode = true;
            belt.SetTutorialNumbers(new int[] { 2, 3, 2, 3 });
        }
    }

    IEnumerator BoxSpawn(BoxType box)
    {
        foreach (var belt in FindObjectsOfType<ConveyorBelt>())
        {
            if (belt == FindObjectsOfType<ConveyorBelt>()[0] && boxTypes.Count > 0)
            {
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
                yield return new WaitForSeconds(1.5f);
                belt.QueueBoxSpawn(box.prefab, BoxEffectType.YellowEffect);
            }
        }
    }

    public void ForceSpecialBoxPhase()
    {
        RemoveAllMonsters();

        // 1. Force all belts to spawn only 4s, and one x2 box
        foreach (var belt in FindObjectsOfType<ConveyorBelt>())
        {
            // Only on first belt, spawn the box next time a slot loops!
            if (belt == FindObjectsOfType<ConveyorBelt>()[0] && boxTypes.Count > 0)
            {
                belt.ClearTutorialNumbers();
                var x2Box = boxTypes.Find(b => b.effectType == BoxEffectType.YellowEffect);
                if (x2Box != null)
                {
                    StartCoroutine(BoxSpawn(x2Box));
                }
            }
            else
            {
                belt.ClearTutorialNumbers();
                belt.isTutorialMode = true;
                belt.SetTutorialNumbers(new int[] { 4, 4, 4, 4 });
            }

        }

        // 2. Force spawn customer for '4'
        MonsterType basicType = monsterTypes.Find(m => m.operationType == OperationType.BasicClient);
        if (basicType != null && spawnPoints.Length > 0)
        {
            GameObject monster = Instantiate(basicType.prefab, spawnPoints[0].position, Quaternion.identity);
            var monsterScript = monster.GetComponent<Monster>();
            var risingScript = monster.GetComponent<RisingMonster>();
            if (risingScript != null)
            {
                risingScript.targetLine = targetLine;
                risingScript.speed = monsterSpeed * 0.5f; // Slower for tutorial
                risingScript.stayTime = monsterStayTime * 3f; // Much longer time
                risingScript.startY = monsterStartY;
                risingScript.InitializeRising(targetLine);
            }
            if (monsterScript != null)
            {
                int result = 4;
                monsterScript.Initialize($"Vull menjar-me el nombre {result}", result, basicType.points, basicType.operationType);
                monsterScript.orderTime = float.PositiveInfinity;
            }
        }
    }

    // Utility to clear monsters
    public void RemoveAllMonsters()
    {
        foreach (var m in FindObjectsOfType<Monster>())
            Destroy(m.gameObject);
    }

    void GameOver()
    {
        gameActive = false;
        Debug.Log($"Game Over! Final Score: {score}");

        SceneManager.LoadScene("GameOverScene");

        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.SetFloat("FinalGameTime", gameTime);
        PlayerPrefs.SetInt("FinalCompletedOrders", completedOrders);
    }
}