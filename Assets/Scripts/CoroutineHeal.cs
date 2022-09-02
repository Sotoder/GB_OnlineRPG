using System.Collections;
using UnityEngine;

public class CoroutineHeal: MonoBehaviour
{
    [SerializeField] private float _maxHP = 100;
    [SerializeField] private Health _health;

    private bool _isHealing;
    private bool _isHealOnPause;
    private Coroutine _healingCoroutine;

    private void Awake()
    {
        _health = new Health(_maxHP);
        _health.OnMaxValue += ResetHealth;
    }

    private void Update()
    {
        if (_isHealing) return;

        if(!_isHealOnPause)
        {
            _healingCoroutine = StartCoroutine(Healing());
        }

    }

    private void ResetHealth()
    {
        _isHealing = false;
        StopCoroutine(_healingCoroutine);
        StartCoroutine(Wait());
        _health.Value = 0;
    }

    private void Pause()
    {
        _isHealing = false;
        StopCoroutine(_healingCoroutine);
        StartCoroutine(Wait());
    }

    private IEnumerator Healing()
    {
        _isHealing = true;

        var time = 0f;

        while (_isHealing)
        {
            if (time >= 3f)
            {
                Pause();
            }
            else
            {
                _health.Value += 5f;
                time += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
            
        }
    }

    private IEnumerator Wait()
    {
        _isHealOnPause = true;
        yield return new WaitForSeconds(3f);
        _isHealOnPause = false;
    }

    private void OnDestroy()
    {
        _health.OnMaxValue -= ResetHealth;
    }
}