using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSpiderwebUI : SingletonMonoBehaviour<ShowSpiderwebUI>
{
    [SerializeField] private GameObject _spider_barout;
    [SerializeField] private Image _spider_bar;
     private float _testfloat = 0;
    private const float bar_fillAmount = 0.0714285714f;
    private void Start()
    {
    }

    private void Update()
    {
    }

    public void ValueSet(float i)
    {
        if (0 <= i && i <= 1)
        {
            _spider_bar.fillAmount = Normalizevalue(i);
        }else if (1 < i)
        {
            _spider_bar.fillAmount = 1;
        }else if (i < 0)
        {
            _spider_bar.fillAmount = 0;
        }
    }

    public void ShowSpiderwebSlider(bool _bool)
    {
        _spider_barout.gameObject.SetActive(_bool);
    }

    private float Normalizevalue(float i)
    {
        if (i == 1) {return 1; }
        return bar_fillAmount * (int)(i / bar_fillAmount);
    }
}