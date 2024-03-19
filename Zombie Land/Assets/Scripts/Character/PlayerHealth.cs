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
        _isDead = true;

        _pm.enabled = false;
        _ps.enabled = false;

        Manager.Default.DefeatMenu();
    }

    public void RecieveDMG(float dmg)
    {
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

        BloodOverlayManager.Default.AddEffectMod(0.2f);

        if (_healthBarController && IsLocalPlayer)
            _healthBarController.ReciveDMG(damage, _maxHP);

        if (_currentHP.Value - damage <= 0 && !_isDead)
        {
            Die();
        }
    }
}