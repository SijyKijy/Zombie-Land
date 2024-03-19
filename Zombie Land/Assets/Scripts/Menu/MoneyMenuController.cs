using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyMenuController : MonoBehaviour
{
    [SerializeField] private TMP_Text _moneyTextField;

    [SerializeField] private float _changeSpeed = 1f;

    private int _currentMoney, _tempMoney;
    private Coroutine _moneyUpdater;

    private void Start()
    {
        _currentMoney = Manager.IsDebug ? 100000 : PlayerPrefs.GetInt("Money", 0);
        _tempMoney = _currentMoney;
        _moneyTextField.text = _tempMoney.ToString();
    }

    private IEnumerator CUpdateMoneyAmount()
    {
        var lerp = 0f;

        while (_tempMoney != _currentMoney)
        {
            lerp += Time.deltaTime / _changeSpeed;
            _tempMoney = (int)Mathf.Lerp(_tempMoney, _currentMoney, lerp);
            _moneyTextField.text = _tempMoney.ToString();

            yield return null;
        }

        _moneyTextField.text = _tempMoney.ToString();
    }

    public void UpdateMoney(int difference)
    {
        if (!gameObject.activeInHierarchy)
            return;

        _currentMoney += difference;
        PlayerPrefs.SetInt("Money", _currentMoney);

        if (_moneyUpdater != null)
        {
            StopCoroutine(_moneyUpdater);
        }

        StartCoroutine(CUpdateMoneyAmount());
    }

    public int GetCurrentMoney()
    {
        return _currentMoney;
    }

    #region Singleton

    public static MoneyMenuController Default { get; private set; }

    private void Awake()
    {
        Default = this;
    }

    #endregion
}