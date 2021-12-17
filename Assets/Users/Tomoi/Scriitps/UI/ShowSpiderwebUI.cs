using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSpiderwebUI : SingletonMonoBehaviour<ShowSpiderwebUI>
{
    [SerializeField]
    private Slider _slider;

    private void Start()
    {
    }

    public void ValueSet(float i)
    {
        if (0 <= i && i <= 1)
        {
            _slider.value = i;
        }else if (1 < i)
        {
            _slider.value = 1;
        }else if (i < 0)
        {
            _slider.value = 0;
        }
    }

    public void ShowSpiderwebSlider(bool _bool)
    {
        _slider.gameObject.SetActive(_bool);
    }
}