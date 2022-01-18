using JetBrains.Annotations;
using nn.util;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;

public class TresureBox : MonoBehaviour,IActionable
{
    private Animator _animator;
    /// <summary>両手で掴んでいる状態</summary>
    private bool isOpen = false;
    /// <summary>宝箱が空いているかどうか</summary>
    private bool isOpened = false;

    private static readonly int Open = Animator.StringToHash("Open");
    public bool _isOutline { get; private set; }
    public HandType RequireHand { get; private set; }
    public List<GameObject> _Objects;

    /// <summary>腕を振り上げる際の成功値の角度</summary>
    [SerializeField]private float _successRotation;
    /// <summary>腕を振り上げる際の加速度の成功値の速度</summary>
    [SerializeField]private float _successAccel;
    //宝箱を開くときの判定に使用する初期値用の変数
    private Quaternion _RightJoyCon;
    private Quaternion _LeftJoyCon;
    private Quaternion _quaternion;
    private Float4 _float4;
    private Transform _CheckRotation_1;
    private Transform　_CheckRotation_2;
    
    void Start()
    {
        _animator   = GetComponent<Animator>();
        
        _isOutline = true;
        RequireHand = HandType.Both;
        //子要素がある時のみ
        GetChild(gameObject.transform);
        
    }
    bool GetChild(Transform _transform)
    {
        if (_transform.childCount <= 0){return false;}

        for (int i = 0; i < _transform.childCount; i++)
        {
            _Objects.Add(_transform.GetChild(i).gameObject);
            GetChild(_transform.GetChild(i));
        }
        return true;
    }
    void Update()
    {

        if (isOpen)
        {
            //開く動作
            TresureBoxOpen();
        }

    }

    public void Action()
    {
        if(!isOpen && !isOpened)
        {
            isOpen = true;

            SwitchInputController.Instance.RightJoyConRotaion.GetQuaternion(ref _float4);
            _RightJoyCon.Set(_float4.x,_float4.y,_float4.z,_float4.w);
            SwitchInputController.Instance.LeftJoyConRotaion.GetQuaternion(ref _float4);
            _LeftJoyCon.Set(_float4.x,_float4.y,_float4.z,_float4.w);
            
        }
    }

    public void DeAction()
    {
        isOpen = false;
    }

    private void TresureBoxOpen()
    {
        SwitchInputController.Instance.RightJoyConRotaion.GetQuaternion(ref _float4);
        _quaternion.Set(_float4.x,_float4.y,_float4.z,_float4.w);
        Debug.Log(Mathf.Abs(_quaternion.eulerAngles.x - _RightJoyCon.eulerAngles.x));
        Debug.Log("RJoyConAccel.x :" + SwitchInputController.Instance.RJoyConAccel.x);
        if (Mathf.Abs(_quaternion.eulerAngles.x - _RightJoyCon.eulerAngles.x) >= _successRotation && SwitchInputController.Instance.RJoyConAccel.x >=_successAccel)
        {
            SwitchInputController.Instance.LeftJoyConRotaion.GetQuaternion(ref _float4);
            _quaternion.Set(_float4.x,_float4.y,_float4.z,_float4.w);

            if (Mathf.Abs(_quaternion.eulerAngles.x - _LeftJoyCon.eulerAngles.x) >= _successRotation && SwitchInputController.Instance.LJoyConAccel.x >=_successAccel)
            {
                _animator.SetTrigger(Open);
                Gamemaneger.Instance.OnClear();
                //完全に開いた状態でtrue
                isOpened = true;
            }
        }
    }



    public void ShowOutline()
    {
        foreach (GameObject obj in _Objects)
        {
            obj.layer = 9;
        }
    }

    public void HideOutline()
    {
        foreach (GameObject obj in _Objects)
        {
            obj.layer = 0;
        }
    }
}
