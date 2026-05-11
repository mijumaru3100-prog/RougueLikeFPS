using UnityEngine;
using System.Collections.Generic;
public class Battle : Stage
{
    public PlayerManager manager;
    [Header("ターゲット（ドア等）")]
    public GameObject[] targetObjects; 
    [Header("敵プレハブ")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    private bool _isBattleStarted = false;
    private bool _finished = false;
    public override void ResetStage()
    {
        _finished = false;
        _isBattleStarted = false;
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        _activeEnemies.Clear();
    }
    public override void StartStage()
    {
        if (_finished) return;
        if (_isBattleStarted) return;
        _isBattleStarted = true;
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) obj.SetActive(true); 
        }
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab が設定されていません。");
            EndBattle();
            return;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("SpawnPoints が設定されていません。");
            EndBattle();
            return;
        }
        foreach (Transform pt in spawnPoints)
        {
            if (pt == null) continue;
            GameObject enemy = Instantiate(enemyPrefab, pt.position, pt.rotation);
            _activeEnemies.Add(enemy);
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
            {
                if (manager != null && manager.Player != null)
                {
                    ai.target = manager.Player.transform;
                }
                else
                {
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        ai.target = playerObj.transform;
                    }
                }
            }
        }
        Debug.Log("<color=red>戦闘開始</color>");
    }
        void Update()
    {
        if (!_isBattleStarted) return;
        if (_finished) return;
        _activeEnemies.RemoveAll(item => item == null);
        if (_activeEnemies.Count == 0)
        {
            EndBattle();
        }
    }
        void EndBattle()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) obj.SetActive( false); 
        }
        Debug.Log("<color=green>戦闘終了。先へ進めます。</color>");
        _finished = true; 
    }
}
