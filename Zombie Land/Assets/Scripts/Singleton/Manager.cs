using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    [SerializeField] private Camera _cameraMain;

    [SerializeField] private Transform _plCharaTransform;

    [SerializeField] private Curtain
        _curtain,
        _defeatCurtain;

    [SerializeField] private GameObject
        _pauseInterface,
        _defeatInterface,
        _winInterface;

    public static bool IsDebug = true;

    private bool _isPauseActive;

    #region Singleton

    public static Manager Default { get; private set; }

    #endregion

    private void Awake()
    {
        Default = this;

        if (_cameraMain == null)
            _cameraMain = Camera.main;
    }

    private void Start()
    {
        _curtain.CloseCurtain();
        _curtain.OnClose += ResumeTime;
        _defeatCurtain.OnOpen += PauseTime;

        AudioManager.Default.PlayBGPreset(AudioManager.Presets.Game);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu();
        }
    }

    public Camera GetMainCamera()
    {
        return _cameraMain;
    }

    public Transform GetPlayerCharacterTransform()
    {
        return _plCharaTransform;
    }

    public void PauseMenu()
    {
        if (_isPauseActive)
        {
            _isPauseActive = false;

            _curtain.CloseCurtain();
            _pauseInterface.SetActive(false);
        }
        else
        {
            PauseTime();
            _isPauseActive = true;

            _curtain.OpenCurtain();
            _pauseInterface.SetActive(true);
        }
    }

    public void DefeatMenu()
    {
        _defeatCurtain.OpenCurtain();
        _defeatInterface.SetActive(true);
    }

    private void PauseTime()
    {
        if (IsDebug)
            return;

        Time.timeScale = 0;
    }

    private void ResumeTime()
    {
        if (IsDebug)
            return;

        Time.timeScale = 1;
    }

    public void GotoMenu()
    {
        if (IsDebug)
            return;

        ResumeTime();
        SceneManager.LoadScene(0);
    }

    public void NextLevel()
    {
        if (IsDebug)
            return;

        ResumeTime();
        SceneManager.LoadScene(1);
    }

    public void WinMenu()
    {
        _curtain.OpenCurtain();
        _winInterface.SetActive(true);
    }
}