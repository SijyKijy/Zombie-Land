using System.Collections;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;

public class ZombieController : NetworkBehaviour, IDamagable
{
    private const float MIN_ATTACK_DIST = 2f, ATTACK_DELAY = 0.3f;

    [SerializeField] private float
        _maxHP,
        _attackSpeedTime,
        _damageAmount;

    [SerializeField] private Animator _animator;

    [SerializeField] private HealthBarController _healthBarController;

    [SerializeField] private ZombieRagdollController _ragdollController;

    [SerializeField] private GameObject _moneyInstance;

    [SerializeField] private int
        _dropChance,
        _dropAmount;

    [SerializeField] private Renderer _renderer;

    private readonly NetworkVariable<float> _currentHP = new();

    private float currentAttackTime;

    private Transform destinationTarget;

    private void Update()
    {
        currentAttackTime -= Time.deltaTime;

        if (!destinationTarget)
            return;

        var dist = Vector3.Distance(transform.position, destinationTarget.position);
        if (dist <= MIN_ATTACK_DIST && currentAttackTime <= 0)
        {
            Attack();
        }
    }

    public void RecieveDMG(float dmg)
    {
        ReceiveDamageServerRpc(dmg);
    }

    public void Die()
    {
        if (_ragdollController)
        {
            _ragdollController.SetRagdoll(true);
        }

        var tempChance = Random.Range(_dropChance, _dropAmount);

        for (var i = 0; i < tempChance; i++)
        {
            Instantiate(_moneyInstance, transform.position, Quaternion.identity);
        }

        StartCoroutine(CDissolve());

        if (IsServer)
        {
            LevelManager.Default.ZombieKilled();
            Destroy(gameObject, 4f);
        }

        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _currentHP.Value = _maxHP;
        }

        SetDestinationTarget();
    }

    [Rpc(SendTo.Server)]
    public void ReceiveDamageServerRpc(float damage)
    {
        var newHp = _currentHP.Value - damage;
        _currentHP.Value = newHp;
        ReceiveDamageClientRpc(damage, newHp);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveDamageClientRpc(float damage, float newHp)
    {
        if (_healthBarController)
            _healthBarController.ReciveDMG(damage, _maxHP);

        if (newHp <= 0)
        {
            Die();
        }
    }

    private void Attack()
    {
        currentAttackTime = _attackSpeedTime;

        if (destinationTarget.TryGetComponent(out IDamagable damageTarget))
        {
            StartCoroutine(CDelayDamage(damageTarget, ATTACK_DELAY));

            if (!_animator)
                return;

            _animator.SetTrigger("Attack");
        }
    }

    private IEnumerator CDelayDamage(IDamagable target, float delay)
    {
        yield return new WaitForSeconds(delay);
        target.RecieveDMG(_damageAmount);
    }

    private void SetDestinationTarget()
    {
        destinationTarget = Manager.Default.GetPlayerCharacterTransform();
        if (TryGetComponent(out AIDestinationSetter aiDestinationSetter))
        {
            aiDestinationSetter.target = destinationTarget;
        }
    }

    public void DieWithoutCallback()
    {
        if (_ragdollController)
        {
            _ragdollController.SetRagdoll(true);
            StartCoroutine(CDissolve());

            Destroy(gameObject, 4f);
            enabled = false;
        }
    }

    private IEnumerator CDissolve()
    {
        float timeElapsed = 0, duration = 2, startCutoffHeight = _renderer.material.GetFloat("_CutoffHeight");

        while (timeElapsed < duration)
        {
            _renderer.material.SetFloat("_CutoffHeight", Mathf.Lerp(startCutoffHeight, 0, timeElapsed / duration));
            timeElapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}