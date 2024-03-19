using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private RecoilCompressor _recoilCompressor;

    [SerializeField] private WeaponHolder _weaponHolder;

    [SerializeField] private AudioClip _clipReload;

    private WeaponInfo _currentWeaponInfo;

    private float _elapsedTime;

    private void Update()
    {
        if (!IsOwner)
            return;

        _elapsedTime -= Time.deltaTime;

        if (Input.GetMouseButton(0) && _currentWeaponInfo.IsAbleToShoot())
        {
            HandleShooting();
            HandleFX();
        }
        else
        {
            HandleEndShooting();
        }
    }

    private void OnEnable()
    {
        _weaponHolder.WeaponChanged += SetUpWeapon;
    }

    private void OnDisable()
    {
        _weaponHolder.WeaponChanged -= SetUpWeapon;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) enabled = false;

        _currentWeaponInfo = _weaponHolder.GetCurrentWeapon();
    }

    private void SetUpWeapon(WeaponInfo _weaponInfo)
    {
        _currentWeaponInfo = _weaponInfo;

        UIWeaponManager.Default.SetActiveWeapon(_currentWeaponInfo._weaponInfo._weaponParams.Index);
        _elapsedTime = 0.5f;

        AudioManager.Default.PlaySoundFXAtPoint(_clipReload, transform);
        AudioManager.Default.DestroySingleSources();
    }

    private void HandleEndShooting()
    {
        if (_currentWeaponInfo && _currentWeaponInfo._weaponInfo._muzzleFlash)
            _currentWeaponInfo._weaponInfo._muzzleFlash.Stop();

        AudioManager.Default.DestroySingleSources();
    }

    private void HandleFX()
    {
        if (_currentWeaponInfo._weaponInfo._muzzleFlash)
            _currentWeaponInfo._weaponInfo._muzzleFlash.Play();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void ShootServerRpc(Vector3 position, Quaternion rotation) // TODO: Скорее всего не нужен
    {
        ShootClientRpc(position, rotation);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShootClientRpc(Vector3 position, Quaternion rotation) // TODO: По хорошгму передать id оружимя и уже от него спавшить нужные пули
    {
        var weap = _weaponHolder.GetCurrentWeapon();
        Instantiate(weap._weaponInfo._bulletInstance, position, rotation);
        _recoilCompressor.AddRecoil(weap._weaponInfo._weaponParams._recoilStrength);
    }

    private void HandleShooting()
    {
        if (_elapsedTime <= 0)
        {
            _elapsedTime = _currentWeaponInfo._weaponInfo._weaponParams._attackSpeed;
            
            ShootClientRpc(_currentWeaponInfo._weaponInfo._bulletSpawnPoint.position, transform.rotation);
            
            CameraShaker.Default.Shake(_currentWeaponInfo._weaponInfo._weaponParams._reciolFrequency,
                _currentWeaponInfo._weaponInfo._weaponParams._recoilDuration);

            _currentWeaponInfo.ReduceAmmo(1);

            if (_currentWeaponInfo._weaponInfo._weaponParams.ShotSound)
                if (_currentWeaponInfo._weaponInfo._weaponParams.isAudioSingle)
                    AudioManager.Default.PlaySoundFXAtPointSingle(
                        _currentWeaponInfo._weaponInfo._weaponParams.ShotSound, transform);
                else
                    AudioManager.Default.PlaySoundFXAtPoint(_currentWeaponInfo._weaponInfo._weaponParams.ShotSound,
                        transform);
        }
    }
}