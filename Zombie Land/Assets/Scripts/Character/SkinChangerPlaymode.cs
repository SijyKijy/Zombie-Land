using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkinChangerPlaymode : NetworkBehaviour
{
    private const string PREFS_NAME = "Skin";

    [SerializeField] private List<GameObject> _skinPool;

    private readonly NetworkVariable<int> _currentSkin = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Start()
    {
        var skinId = _currentSkin.Value == -1 ? GetSkinIdFromPrefs() : _currentSkin.Value;
        UpdateSkin(skinId);
    }

    private int GetSkinIdFromPrefs()
    {
        for (var i = 0; i < _skinPool.Capacity; i++)
        {
            if (PlayerPrefs.GetInt(PREFS_NAME + i, 0) != 2) continue;
            return i;
        }

        return 0;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _currentSkin.Value = GetSkinIdFromPrefs();
        }

        _currentSkin.OnValueChanged += OnSkinChanged;
        base.OnNetworkSpawn();
    }

    private void OnSkinChanged(int previousValue, int newValue)
    {
        UpdateSkin(newValue);
    }

    public void UpdateSkin(int skinId)
    {
        foreach (var skin in _skinPool)
        {
            skin.SetActive(false);
        }

        _skinPool[skinId].SetActive(true);
    }
}