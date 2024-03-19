using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : NetworkBehaviour
{
    [SerializeField] private List<LevelSettings> _settings;

    [SerializeField] private Transform _playerTransform;

    [SerializeField] private TMP_Text _progressBarTextfield, _rewardTextfield;

    [SerializeField] private Image _progressBarFill;

    [SerializeField] private int _maxZombieInstances;

    [SerializeField] private Transform[] _spawnPoints, _plSpawnPoints;

    public Transform[] PlSpawnPoints => _plSpawnPoints;

    private LevelSettings _currentLevelSettings;

    private int
        _targetFrags,
        _currentFrags,
        _currentLevel,
        _currentZombieInstances;

    private bool isAlreadyWin;

    private void Start()
    {
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer || !IsOwner)
        {
            enabled = false;
            //gameObject.SetActive(false); // TODO: Возможно потребуется выключить
            return;
        }

        _playerTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
        Init();
    }

    private void Init()
    {
        _currentLevel = PlayerPrefs.GetInt("Level", 0);
        _currentLevelSettings = _settings[Mathf.Min(_settings.Count - 1, _currentLevel)];
        _targetFrags = _currentLevelSettings.TargetFrags;

        for (var i = 0; i < _maxZombieInstances; i++)
        {
            SpawnZombie();
        }

        _playerTransform.position = _plSpawnPoints[Random.Range(0, _plSpawnPoints.Length)].position;

        UpdateProgressBar();
    }

    private void SpawnZombie()
    {
        if (!IsServer)
            return;

        var enemy = Instantiate(_currentLevelSettings.ZombiePool[Random.Range(0, _currentLevelSettings.ZombiePool.Count)], _spawnPoints[Random.Range(0, _spawnPoints.Length)].position, Quaternion.identity);
        enemy.GetComponent<NetworkObject>().Spawn(true);
        _currentZombieInstances++;
    }

    public void ZombieKilled()
    {
        _currentFrags++;
        _currentZombieInstances--;

        UpdateProgressBar();
        SpawnZombie();

        if (_currentFrags >= _targetFrags && !isAlreadyWin)
        {
            isAlreadyWin = true;
            Win();
        }
    }

    private void Win()
    {
        var currReward = _currentLevelSettings.Reward;
        MoneyMenuController.Default.UpdateMoney(currReward);
        _rewardTextfield.text = "+" + currReward;
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level", 0) + 1);

        var tempZombies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
        foreach (var zombie in tempZombies)
        {
            if (zombie.TryGetComponent(out ZombieController tempZC))
            {
                tempZC.DieWithoutCallback();
            }
        }

        StartCoroutine(CWin());
    }

    private IEnumerator CWin()
    {
        yield return new WaitForSecondsRealtime(2f);
        Manager.Default.WinMenu();
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 0f;
    }

    private void UpdateProgressBar()
    {
        return; // TODO: Доделать
        _progressBarFill.fillAmount = (float)_currentFrags / _targetFrags;
        _progressBarTextfield.text = _currentFrags + " / " + _targetFrags;
    }

    #region Singleton

    public static LevelManager Default { get; private set; }

    private void Awake()
    {
        Default = this;
    }

    #endregion
}