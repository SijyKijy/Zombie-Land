using UnityEngine;

public class FlameBullet : MonoBehaviour
{
    [SerializeField] private float _dmg, _lifeTime;
    [SerializeField] private string _enemyTag;
    [SerializeField] private WeaponParams _weaponInfo;

    private readonly string PREFS_WEAPON_NAME = "Weapon";
    private readonly string PREFS_UPGRADE_NAME = "Upgrade";

    private void Start()
    {
        Destroy(gameObject.transform.parent.gameObject, _lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == _enemyTag)
        {
            if (other.gameObject.TryGetComponent(out IDamagable _IDamagable))
            {
                _IDamagable.RecieveDMG(_weaponInfo.InitialDMG + _weaponInfo.DamageMod * PlayerPrefs.GetInt(PREFS_WEAPON_NAME + _weaponInfo.Index + PREFS_UPGRADE_NAME + 0));
            }
        }
    }
}