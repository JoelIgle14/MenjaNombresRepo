using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject monsterPrefab;
    public Transform[] spawnPoints;
    public Transform targetLine;
    public float spawnDelay = 2f;
    public int maxMonsters = 3;

    [Header("Configuración de monstruos")]
    public float monsterSpeed = 2f;
    public float monsterStayTime = 3f;
    public float monsterStartY = -10f;

    private bool gameActive = true;

    void Start()
    {
        StartCoroutine(SpawnWaveLoop());
    }

    IEnumerator SpawnWaveLoop()
    {
        while (gameActive)
        {
            List<GameObject> currentWave = new List<GameObject>();

            // Crear todos los monstruos sincronizados
            for (int i = 0; i < maxMonsters && i < spawnPoints.Length; i++)
            {
                Transform spawn = spawnPoints[i];
                GameObject monster = Instantiate(monsterPrefab, spawn.position, Quaternion.identity);

                var risingScript = monster.GetComponent<RisingMonster>();
                if (risingScript != null)
                {
                    risingScript.targetLine = targetLine;
                    risingScript.speed = monsterSpeed;
                    risingScript.stayTime = monsterStayTime;
                    risingScript.startY = monsterStartY;
                }

                currentWave.Add(monster);
            }

            // Esperar hasta que todos se destruyan
            while (currentWave.Exists(m => m != null))
                yield return null;

            // Esperar antes de la siguiente oleada
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
