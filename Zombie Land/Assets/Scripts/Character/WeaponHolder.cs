using System;
using Unity.Netcode;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
    [SerializeField] private WeaponInfo[] _weaponInstances;

    private readonly NetworkVariable<int> _currentWeapon = new();
    public Action<WeaponInfo> WeaponChanged;

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetButtonDown("WeaponSwitchNext"))
            NextWeapon();
        if (Input.GetButtonDown("WeaponSwitchPrevious"))
            PreviousWeapon();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            UpdateWeaponClientRpc(_currentWeapon.Value);
            return;
        }

        RequestWeaponChangeServerRpc(0, RpcTarget.Server);
    }

    [ContextMenu("Next Weapon")]
    private void NextWeapon()
    {
        var currentWeapon = _currentWeapon.Value;
        do
        {
            currentWeapon = ++currentWeapon % _weaponInstances.Length;
        } while (PlayerPrefs.GetInt("Weapon" + currentWeapon) != 1);

        RequestWeaponChangeServerRpc(currentWeapon, RpcTarget.Server);
    }

    [ContextMenu("Previous Weapon")]
    private void PreviousWeapon()
    {
        var currentWeapon = _currentWeapon.Value;
        do
        {
            currentWeapon = (currentWeapon + _weaponInstances.Length - 1) % _weaponInstances.Length;
        } while (PlayerPrefs.GetInt("Weapon" + currentWeapon) != 1);

        RequestWeaponChangeServerRpc(currentWeapon, RpcTarget.Server);
    }

    private void UnequipAll()
    {
        foreach (var weaponInfo in _weaponInstances)
        {
            weaponInfo.gameObject.SetActive(false);
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false, AllowTargetOverride = true)]
    public void RequestWeaponChangeServerRpc(int weaponIndex, RpcParams rpcParams)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out var client))
            return;

        var player = client.PlayerObject.GetComponentInChildren<WeaponHolder>(); // TODO: Грамотнее искать челикса
        if (!player)
            return;

#if UNITY_EDITOR
        Debug.Log($"Swap to server (ClientId: {rpcParams.Receive.SenderClientId} | WeaponId: {weaponIndex})");
#endif

        player._currentWeapon.Value = weaponIndex;
        player.UpdateWeaponClientRpc(weaponIndex);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateWeaponClientRpc(int weaponIndex)
    {
#if UNITY_EDITOR
        Debug.Log($"Swap to client (WeapoonId: {weaponIndex} | CurrentClient: {NetworkManager.LocalClientId} | OwnerId: {NetworkObject.OwnerClientId})");
#endif

        UnequipAll();
        _weaponInstances[weaponIndex].gameObject.SetActive(true);
        WeaponChanged?.Invoke(_weaponInstances[weaponIndex]);
    }

    public WeaponInfo GetCurrentWeapon()
    {
        return _weaponInstances[_currentWeapon.Value];
    }
}