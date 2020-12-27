using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text waveUI;
    public RobotStats playerStats;
    public GameObject waveSpawner;
    public GameObject enemyBasic;
    public GameObject enemySin;
    public GameObject enemyTank;
    public GameObject batteryDrop;
    public GameObject healthDrop;
    public GameObject powerupSpawn;

    private int wave;

    void Start()
    {
        Cursor.visible = false;
        wave = -1;
        UpdateWaveUI();
        StartCoroutine(SpawnInit());
    }

    IEnumerator SpawnInit()
    {
        float timeDelay = 3f;
        while (timeDelay > 0f)
        {
            yield return null;
            timeDelay -= Time.deltaTime;
        }

        SpawnNextWave();
    }

    void UpdateWaveUI()
    {
        waveUI.text = "Wave : " + (wave + 1).ToString();
    }

    void SpawnPowerup(GameObject pw)
    {
        Instantiate(pw, powerupSpawn.transform.position, pw.transform.rotation);
    }

    public void SpawnNextWave()
    {
        wave++;
        UpdateWaveUI();
        int division = wave / 3;
        int modulo = wave % 3;

        switch(modulo)
        {
            case 0:
                if (division >= 1)
                {
                    if (playerStats.health / playerStats.maximumHealth <= playerStats.fuel / playerStats.maximumHealth)
                    {
                        SpawnPowerup(healthDrop);
                    }
                    else
                    {
                        SpawnPowerup(batteryDrop);
                    }
                }
                SpawnEnemy(enemyBasic, division);
                break;
            case 1:
                SpawnEnemy(enemySin, division);
                break;
            case 2:
                SpawnEnemy(enemyTank, division);
                break;
            default:
                break;
        }
    }

    void SpawnEnemy(GameObject enemy, int extra)
    {
        GameObject g = Instantiate(enemy, waveSpawner.transform.position, enemy.transform.rotation);
        Enemy e = g.GetComponent<Enemy>();
        e.gm = this;
        e.playerTarget = playerStats.gameObject;
        e.health += (e.health / 3) * extra;
        Gun gun = g.transform.GetComponentInChildren<Gun>();
        gun.fireRate -= (gun.fireRate / 3) * extra;
    }
}
