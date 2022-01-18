using nn.util;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Doar : MonoBehaviour,IActionable
{
    /// <summary>両手で掴んでいる状態</summary>
    private bool isOpen = false;
    /// <summary>宝箱が空いているかどうか</summary>
    private bool isOpened = false;

    public bool _isOutline { get; private set; }
    public HandType RequireHand { get; private set; }

    /// <summary>腕を振り上げる際の成功値の角度</summary>
    [SerializeField]private float _successRotation = 0.3f;
    /// <summary>腕を振り上げる際の加速度の成功値の速度</summary>
    [SerializeField]private float _successAccel= 0.3f;
    //宝箱を開くときの判定に使用する初期値用の変数
    private Quaternion _RightJoyCon;
    private Quaternion _LeftJoyCon;
    private Quaternion _quaternion;
    private Float4 _float4;


    /// <summary>ドアの初期値から開いた際の位置までに移動する距離</summary>
    [SerializeField, Header("ドアの初期値から開いた際の位置までに移動する距離")] private float _rangeOpenDoorFully = 200f;

    [SerializeField] private GameObject _rightDoar;
    [SerializeField] private GameObject _leftDoar;
    //ドアの初期位置を保持
    private Vector3 _rightDoarDefaultPosition;
    private Vector3 _leftDoarDefaultPosition;

    //ドアの開いた位置を保持
    private Vector3 _rightDoarAfterPosition;
    private Vector3 _leftDoarAfterPosition;

    private float _rightProgressRate = 0f;
    private float _leftProgressRate = 0f;
    void Start()
    {

        _isOutline = true;
        RequireHand = HandType.Both;

        _rightDoarDefaultPosition = _rightDoar.transform.localPosition;
        _leftDoarDefaultPosition = _leftDoar.transform.localPosition;

        _rightDoarAfterPosition.Set(_rightDoarDefaultPosition.x + _rangeOpenDoorFully, _rightDoarDefaultPosition.y,
            _rightDoarDefaultPosition.z);
        _leftDoarAfterPosition.Set(_leftDoarDefaultPosition.x - _rangeOpenDoorFully, _leftDoarDefaultPosition.y,
            _leftDoarDefaultPosition.z);
    }

    void Update()
    {

        if (isOpen && !isOpened)
        {
            //開く動作
            DoarBoxOpen();
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


    private float _progressRate = 0f;
    private float angle =0f;
    private void DoarBoxOpen()
    {
        //Joy-Conの入力を取得
        SwitchInputController.Instance.RightJoyConRotaion.GetQuaternion(ref _float4);
        //Joy-Conの入力をQuaternionに変換
        _quaternion.Set(_float4.x,_float4.y,_float4.z,_float4.w);
        //両方のJoy-Conで握った時からの差
        angle = Mathf.Abs(_quaternion.eulerAngles.x - _RightJoyCon.eulerAngles.x);
        //Joy-Conの入力を+0~30の値に変換したものと加速度で判定
        if (Mathf.Clamp(Mathf.Abs((Mathf.Repeat(angle + 180, 360) - 180)),0,30)>= _successRotation &&
            SwitchInputController.Instance.RJoyConAccel.y >= _successAccel &&
            SwitchInputController.Instance.RJoyConAccel.y <= 0.4f)
        {
            _progressRate =
                Mathf.Clamp(Mathf.Abs((_quaternion.eulerAngles.x - _RightJoyCon.eulerAngles.x) / _successRotation), 0f,
                    100f);
            if (_rightProgressRate <= _progressRate){ _rightProgressRate = _progressRate; }
            _rightDoar.transform.localPosition = proportionFromPositionCalculation(_rightDoarDefaultPosition,
                _rightDoarAfterPosition, _rightProgressRate);
        }

        SwitchInputController.Instance.LeftJoyConRotaion.GetQuaternion(ref _float4);
        _quaternion.Set(_float4.x,_float4.y,_float4.z,_float4.w);
        angle = Mathf.Abs(_quaternion.eulerAngles.x - _LeftJoyCon.eulerAngles.x);
        if (Mathf.Clamp(Mathf.Abs((Mathf.Repeat(angle + 180, 360) - 180)), 0, 30) >= _successRotation &&
            SwitchInputController.Instance.LJoyConAccel.y >= _successAccel &&
            SwitchInputController.Instance.LJoyConAccel.y <= 0.4f)
        {
            _progressRate =
                Mathf.Clamp(Mathf.Abs((_quaternion.eulerAngles.x - _LeftJoyCon.eulerAngles.x) / _successRotation), 0f,
                    100f);
            if (_leftProgressRate <= _progressRate){ _leftProgressRate = _progressRate;}
            _leftDoar.transform.localPosition = proportionFromPositionCalculation(_leftDoarDefaultPosition,
                _leftDoarAfterPosition, _leftProgressRate);
        }
        //完全に開いた状態でtrue
        if(_leftProgressRate >= 90 && _rightProgressRate >= 90){isOpened = true;
            Debug.Log("Opend");

            Gamemaneger.Instance.OnClear();
        }
    }

    /// <summary></summary>
    /// <param name="_defaultPosition">初期値の座標</param>
    /// <param name="_afterPosition">移動後の最終的な座標</param>
    /// <param name="proportion">現在の完了までの割合</param>
    Vector3 proportionFromPositionCalculation(Vector3 _defaultPosition,Vector3 _afterPosition,float proportion)
    {
        return Vector3.Lerp(_defaultPosition,_afterPosition,proportion / 100);
    }
    public void ShowOutline()
    {
        _rightDoar.layer = 9;
        _leftDoar.layer = 9;
    }

    public void HideOutline()
    {
        _rightDoar.layer = 0;
        _leftDoar.layer = 0;
    }
}
