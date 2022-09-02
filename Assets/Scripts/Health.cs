using System;
using UnityEngine;

[Serializable]
public class Health
{
    public Action OnMaxValue;

    [SerializeField] private float _value;
    private float _maxValue;

    public Health (float maxValue)
    {
        _maxValue = maxValue;
    }

    public float Value 
    { 
        get => _value;
        set 
        { 
            if(value >= _maxValue)
            {
                _value = _maxValue;
                OnMaxValue?.Invoke();
            }
            else
            {
                _value = value <= 0 ? 0 : value;
            }
        } 
    }
}
