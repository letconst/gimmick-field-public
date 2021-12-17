using nn.hid;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Rock : MonoBehaviour, IActionable
{
    [SerializeField, Header("投げた際の速度の感度"), Range(.1f, 50)]
    private float throwSensitivity;

    private bool _isHoldByPlayer;

    private Rigidbody _selfRig;
    private Collider  _selfCollider;
    private Transform _playerTrf;

    #region 暫定サンプルコード（Joy-Con入力ラッパークラスができたら差し替え・削除）

    private NpadId    _npadId    = NpadId.Invalid;
    private NpadStyle _npadStyle = NpadStyle.Invalid;
    private NpadState _npadState;

    private readonly SixAxisSensorHandle[] _handle = new SixAxisSensorHandle[2];
    private          SixAxisSensorState    _state;
    private          int                   _handleCount;

    private nn.util.Float4 _npadQuaternion;
    private Quaternion     _quaternion;

    public HandType RequireHand { get; private set; }

    #endregion

    private void Start()
    {
        _selfRig      = GetComponent<Rigidbody>();
        _selfCollider = GetComponent<Collider>();
        _playerTrf    = GameObject.FindWithTag("Player").transform;

        RequireHand = HandType.One;
    }

    private void Update()
    {
        NpadSample();

        if (_isHoldByPlayer)
        {
            transform.position = PlayerManager.Instance.PlayerHandTrf.position;
        }
    }

    public void Action()
    {
        // 保持状態にする
        _isHoldByPlayer = true;
        // 重力無効化
        _selfRig.useGravity = false;
        // プレイヤーとの物理判定を無視するレイヤーに切り替え
        gameObject.layer = 3;

        PlayerManager.Instance.HoldObject = this;
    }

    public void DeAction()
    {
        _isHoldByPlayer     = false;
        _selfRig.useGravity = true;
        gameObject.layer    = 0;

        PlayerManager.Instance.HoldObject = null;

        Vector3 angle = /*PlayerManager.Instance.PlayerThrowAngleTrf.position -*/
                        PlayerManager.Instance.PlayerHandTrf.position;

        Vector3 dir = /*Quaternion.Euler(angle) * */Camera.main.transform.forward;
        float acceleration = new Vector3(_state.acceleration.x,
                                         _state.acceleration.y,
                                         _state.acceleration.z).magnitude;

        // 持ってる石を、Joy-Conを振った加速度で飛ばす
        _selfRig.AddForce(dir * acceleration * throwSensitivity, ForceMode.VelocityChange);
    }

    #region 暫定サンプルコード

    private void NpadSample()
    {
        var npadId    = NpadId.Handheld;
        var npadStyle = NpadStyle.None;

        npadStyle = Npad.GetStyleSet(npadId);

        if (npadStyle != NpadStyle.Handheld)
        {
            npadId    = NpadId.No1;
            npadStyle = Npad.GetStyleSet(npadId);
        }

        if (UpdatePadState())
        {
            for (int i = 0; i < _handleCount; i++)
            {
                SixAxisSensor.GetState(ref _state, _handle[i]);
                _state.GetQuaternion(ref _npadQuaternion);
            }
        }
    }

    private bool UpdatePadState()
    {
        NpadStyle handheldStyle = Npad.GetStyleSet(NpadId.Handheld);
        NpadState handheldState = _npadState;

        if (handheldStyle != NpadStyle.None)
        {
            Npad.GetState(ref handheldState, NpadId.Handheld, handheldStyle);

            if (handheldState.buttons != NpadButton.None)
            {
                if ((_npadId != NpadId.Handheld) || (_npadStyle != handheldStyle))
                {
                    this.GetSixAxisSensor(NpadId.Handheld, handheldStyle);
                }

                _npadId    = NpadId.Handheld;
                _npadStyle = handheldStyle;
                _npadState = handheldState;

                return true;
            }
        }

        NpadStyle no1Style = Npad.GetStyleSet(NpadId.No1);
        NpadState no1State = _npadState;

        if (no1Style != NpadStyle.None)
        {
            Npad.GetState(ref no1State, NpadId.No1, no1Style);

            if (no1State.buttons != NpadButton.None)
            {
                if ((_npadId != NpadId.No1) || (_npadStyle != no1Style))
                {
                    this.GetSixAxisSensor(NpadId.No1, no1Style);
                }

                _npadId    = NpadId.No1;
                _npadStyle = no1Style;
                _npadState = no1State;

                return true;
            }
        }

        if ((_npadId == NpadId.Handheld) && (handheldStyle != NpadStyle.None))
        {
            _npadId    = NpadId.Handheld;
            _npadStyle = handheldStyle;
            _npadState = handheldState;
        }
        else if ((_npadId == NpadId.No1) && (no1Style != NpadStyle.None))
        {
            _npadId    = NpadId.No1;
            _npadStyle = no1Style;
            _npadState = no1State;
        }
        else
        {
            _npadId    = NpadId.Invalid;
            _npadStyle = NpadStyle.Invalid;
            _npadState.Clear();

            return false;
        }

        return true;
    }

    private void GetSixAxisSensor(NpadId id, NpadStyle style)
    {
        for (int i = 0; i < _handleCount; i++)
        {
            SixAxisSensor.Stop(_handle[i]);
        }

        _handleCount = SixAxisSensor.GetHandles(_handle, 2, id, style);

        for (int i = 0; i < _handleCount; i++)
        {
            SixAxisSensor.Start(_handle[i]);
        }
    }

    #endregion
}
