using System.Collections;
using System.Collections.Generic;
using nn.util;
using UnityEngine;
using UniRx;
public class Input_Accelerometer_test : MonoBehaviour
{
    [SerializeReference]
    float input_value = 1.5f;
    void Start()
    {
        SwitchInputController.Instance.GetRcontllolerAccelerometer.Subscribe(_f3 =>
            {
                if(Mathf.Abs(_f3.x) > input_value || Mathf.Abs(_f3.y) > input_value || Mathf.Abs(_f3.z) > input_value)
                    Debug.Log("_R" + _f3);
            }
            ).AddTo(this);
        SwitchInputController.Instance.GetLcontllolerAccelerometer.Subscribe(_f3 =>
            {
                if(Mathf.Abs(_f3.x) > input_value || Mathf.Abs(_f3.y) > input_value || Mathf.Abs(_f3.z) > input_value)
                    Debug.Log("_L"+_f3);
            }
        ).AddTo(this);
    }
}
