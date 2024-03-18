using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private const string
        PREFS_FIRST_LAUNCH = "FirstLaunch",
        PREFS_WEAPON_NAME = "Weapon",
        PREFS_UPGRADE_NAME = "Upgrade",
        PREFS_SKIN_NAME = "Skin",
        PREFS_LEVEL = "Level";

    [SerializeField] private GameObject[] _stagesPool;

    [SerializeField] private LevelElement[] _levelElementPool;

    [SerializeField] private Curtain _curtain;

    [SerializeField] private TMP_Text _address;

    private readonly Stack<GameObject> _stagesStack = new();

    private void Start()
    {
        Time.timeScale = 1.0f;
        _stagesStack.Push(_stagesPool[0]);

        ApplyLevelState();
        AudioManager.Default.PlayBGPreset(AudioManager.Presets.MMenu);
    }

    private void ApplyLevelState()
    {
        var targetLevel = PlayerPrefs.GetInt(PREFS_LEVEL, 0);

        for (var i = 0; i < _levelElementPool.Length; i++)
        {
            if (i == targetLevel)
            {
                _levelElementPool[i].SetNext();
            }
            else
            {
                if (i < targetLevel)
                {
                    _levelElementPool[i].SetUnlocked();
                }
                else
                {
                    _levelElementPool[i].SetLocked();
                }
            }
        }
    }

    public void GoToStage(int index)
    {
        _stagesStack.Peek().SetActive(false);
        _stagesStack.Push(_stagesPool[index]);
        _stagesStack.Peek().SetActive(true);

        AudioManager.Default.PlaySoundFXPreset(AudioManager.Presets.Click);
    }

    public void GoToBack()
    {
        _stagesStack.Pop().SetActive(false);
        _stagesStack.Peek().SetActive(true);

        AudioManager.Default.PlaySoundFXPreset(AudioManager.Presets.Click);
    }

    public void StartGame()
    {
        _curtain.OnOpen += LoadGameScene;
        _curtain.OpenCurtain();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(1);
    }

    public void HostGame()
    {
        SetConnectionData();

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            // Проверяем, что загружена нужная сцена
            if (scene.buildIndex == 1)
            {
                // Запускаем хост
                NetworkManager.Singleton.StartHost();
            }

            // Отписываемся от события
            //SceneManager.sceneLoaded -= OnSceneLoaded;
        };

        _curtain.OnOpen += () =>
        {
            var asyncOperation = SceneManager.LoadSceneAsync(1);
            asyncOperation.completed += _ => NetworkManager.Singleton.StartHost();
        };
        _curtain.OpenCurtain();
    }

    private void SetConnectionData()
    {
        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (transport is UnityTransport unityTransport)
        {
            var address = Regex.Replace(_address.text, "[^A-Za-z0-9.:]", "").Split(':');
            var ip = IPAddress.Parse(address[0]).ToString();
            var port = Convert.ToUInt16(address[1]);
            unityTransport.SetConnectionData(ip, port);
        }
    }

    public void JoinGame()
    {
        SetConnectionData();

        _curtain.OnOpen += () =>
        {
            var asyncOperation = SceneManager.LoadSceneAsync(1);
            asyncOperation.completed += _ => NetworkManager.Singleton.StartClient();
        };
        _curtain.OpenCurtain();
    }

    #region Singleton

    public static MenuManager Default { get; private set; }

    private void Awake()
    {
        Default = this;

        Application.targetFrameRate = 60;

        if (PlayerPrefs.GetInt(PREFS_FIRST_LAUNCH, 0) == 0)
        {
            PlayerPrefs.SetInt(PREFS_FIRST_LAUNCH, 1);
            PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 0, 1);
            PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 0 + PREFS_UPGRADE_NAME + 2, 1);
            PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 0 + PREFS_UPGRADE_NAME + 1, 3);
            PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 1, 1);
            PlayerPrefs.SetInt(PREFS_SKIN_NAME + 0, 2);
            //PlayerPrefs.SetInt("Money", 40000);
            //PlayerPrefs.SetInt("Level", 5);
            //PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 2, 1);
            //PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 3, 1);
            //PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 4, 1);
            //PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 5, 1);
            //PlayerPrefs.SetInt(PREFS_WEAPON_NAME + 6, 1);
        }

        PlayerPrefs.SetInt("Weapon5", 0);
    }

    #endregion
}