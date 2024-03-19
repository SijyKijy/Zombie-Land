using System.Collections;
using Cinemachine;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _camera;

    private CinemachineBasicMultiChannelPerlin _noise;
    private IEnumerator _processShakeHolder;

    #region Singleton

    public static CameraShaker Default { get; private set; }

    #endregion

    private void Awake()
    {
        Default = this;
    }

    private void Start()
    {
        _noise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float frequency, float duration)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (_processShakeHolder != null)
            StopCoroutine(_processShakeHolder);
        _processShakeHolder = ProcessShake(frequency, duration);
        StartCoroutine(_processShakeHolder);
    }

    private IEnumerator ProcessShake(float frequency, float duration)
    {
        _noise.m_FrequencyGain = frequency;
        yield return new WaitForSeconds(duration);
        _noise.m_FrequencyGain = 0;
    }
}