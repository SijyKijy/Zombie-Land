using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private float _reduceDelay, _decreaceMod;
    [SerializeField] private Image _actualHealthBar, _deltaHealthBar;

    [SerializeField] private GameObject _popUpDMGInstance;

    private float _targetHealthDelay = 1;

    private void Update()
    {
        _deltaHealthBar.fillAmount = Mathf.Lerp(_deltaHealthBar.fillAmount, _targetHealthDelay, Time.deltaTime * _decreaceMod);
    }

    public void ReciveDMG(float _dmg, float _maxHP)
    {
        var delta = _dmg / _maxHP;
        _actualHealthBar.fillAmount -= delta;

        if (gameObject.activeInHierarchy)
            StartCoroutine(HealthReduceDelay(delta));

        if (_popUpDMGInstance)
            Instantiate(_popUpDMGInstance, transform).GetComponent<UIPopUpDMG>().SetUp(_dmg, false);
    }

    private IEnumerator HealthReduceDelay(float _delta)
    {
        yield return new WaitForSeconds(_reduceDelay);
        _targetHealthDelay -= _delta;
    }

    [ContextMenu("Recieve 23% HP")]
    private void TestRecieveDMG()
    {
        ReciveDMG(23, 100);
    }
}