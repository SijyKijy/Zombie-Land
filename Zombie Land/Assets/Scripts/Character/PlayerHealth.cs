using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamagable
{
    [SerializeField] private float _maxHP;

    [SerializeField] private HealthBarController _healthBarController;

    [SerializeField] private PlayerMovement _pm;

    [SerializeField] private PlayerShooting _ps;

    private readonly NetworkVariable<float> _currentHP = new();

    private bool _isDead;

    public void Die()
    {
        if (Manager.IsDebug)
        {
            DieServerRpc();
            return;
        }

        _pm.enabled = false;
        _ps.enabled = false;

        if (IsLocalPlayer)
            Manager.Default.DefeatMenu();
    }

    [Rpc(SendTo.Server)]
    public void DieServerRpc()
    {
        var m = LevelManager.Default;
        var spawns = m.PlSpawnPoints;

        _currentHP.Value = _maxHP;
        DieClientRpc(_maxHP, spawns[Random.Range(0, spawns.Length)].position);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DieClientRpc(float currentHp, Vector3 spawnPos)
    {
        if (_healthBarController)
            _healthBarController.SetHp(currentHp);

        gameObject.transform.position = spawnPos;
        _isDead = false;
    }

    public void RecieveDMG(float dmg)
    {
        if(!IsOwner)
            return;
        ReceiveDamageServerRpc(dmg);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _currentHP.Value = _maxHP;
        }

        base.OnNetworkSpawn();
    }

    [Rpc(SendTo.Server)]
    public void ReceiveDamageServerRpc(float damage)
    {
        _currentHP.Value -= damage;
        ReceiveDamageClientRpc(damage);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveDamageClientRpc(float damage)
    {
#if UNITY_EDITOR
        Debug.Log($"Receive damage (ClientId: '{NetworkManager.LocalClientId}' | OwnerId: {OwnerClientId} Local: {NetworkManager.LocalClientId} Orig: '{_currentHP}' HP: '{_currentHP.Value - damage}')");
#endif

        if (IsLocalPlayer)
            BloodOverlayManager.Default.AddEffectMod(0.2f);

        if (_healthBarController && IsLocalPlayer)
            _healthBarController.ReciveDMG(damage, _maxHP);

        if (_currentHP.Value - damage <= 0 && !_isDead)
        {
            Die();
        }
    }
}