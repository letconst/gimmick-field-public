using Cysharp.Threading.Tasks;
using nn.util;
using UnityEngine;

public class Doar : MonoBehaviour, IActionable
{
    [SerializeField, Header("開いたときにゲームクリアとするか")]
    private bool isGameClearWhenOpened;

    /// <summary>両手で掴んでいる状態</summary>
    private bool isOpen = false;

    /// <summary>扉が空いているかどうか</summary>
    private bool isOpened = false;

    public bool     _isOutline  { get; private set; }
    public HandType RequireHand { get; private set; }

    /// <summary>腕を振り上げる際の成功値の角度</summary>
    [SerializeField]
    private float _successRotation = 0.3f;

    /// <summary>腕を振り上げる際の加速度の成功値の速度</summary>
    [SerializeField]
    private float _successAccel = 0.3f;

    //宝箱を開くときの判定に使用する初期値用の変数
    private Quaternion _RightJoyCon;
    private Quaternion _LeftJoyCon;
    private Quaternion _quaternion;
    private Float4     _float4;

    private Animator _animator;

    //アウトラインを表示するために保持
    [SerializeField]
    private GameObject _rightDoar;

    [SerializeField]
    private GameObject _leftDoar;

    private float _rightProgressRate = 0f;
    private float _leftProgressRate  = 0f;

    private bool          isLock         = false;
    private LockInterface _lockInterface = null;
    public  bool          isGrab { get; private set; }

    void Start()
    {
        _lockInterface = gameObject.GetComponent<LockInterface>() ?? null;

        _animator   = this.gameObject.GetComponent<Animator>();
        isGrab      = true;
        _isOutline  = true;
        RequireHand = HandType.Both;
    }

    void Update()
    {
        if (_lockInterface == null || !_lockInterface.isLock)
        {
            if (isOpen && !isOpened)
            {
                //開く動作
                DoarBoxOpen();
            }
        }
    }

    public void Action(HandType handType)
    {
        if (!isOpen && !isOpened)
        {
            isOpen = true;

            SwitchInputController.Instance.RightJoyConRotaion.GetQuaternion(ref _float4);
            _RightJoyCon.Set(_float4.x, _float4.y, _float4.z, _float4.w);
            SwitchInputController.Instance.LeftJoyConRotaion.GetQuaternion(ref _float4);
            _LeftJoyCon.Set(_float4.x, _float4.y, _float4.z, _float4.w);

            PlayerHandController
                .SetPositionTo(PlayerHandController.HandPosition.DoorGrab, PlayerHandController.Hand.Left)
                .Forget();

            PlayerHandController
                .SetPositionTo(PlayerHandController.HandPosition.DoorGrab, PlayerHandController.Hand.Right)
                .Forget();
        }
    }

    public void DeAction(HandType handType)
    {
        isOpen = false;

        // ドアを開かずトリガーを離した際は手を元の位置に戻す
        if (!isOpened)
        {
            PlayerHandController
                .SetPositionTo(PlayerHandController.HandPosition.Idle, PlayerHandController.Hand.Left)
                .Forget();

            PlayerHandController
                .SetPositionTo(PlayerHandController.HandPosition.Idle, PlayerHandController.Hand.Right)
                .Forget();
        }
    }

    private float _progressRate = 0f;
    private float angle         = 0f;

    private async void DoarBoxOpen()
    {
        //Joy-Conの入力を取得
        SwitchInputController.Instance.RightJoyConRotaion.GetQuaternion(ref _float4);
        //Joy-Conの入力をQuaternionに変換
        _quaternion.Set(_float4.x, _float4.y, _float4.z, _float4.w);
        //両方のJoy-Conで握った時からの差
        angle = Mathf.Abs(_quaternion.eulerAngles.x - _RightJoyCon.eulerAngles.x);

        //Joy-Conの入力を+0~30の値に変換したものと加速度で判定
        if (Mathf.Clamp(Mathf.Abs((Mathf.Repeat(angle + 180, 360) - 180)), 0, 30) >= _successRotation &&
            SwitchInputController.Instance.RJoyConAccel.y                         >= _successAccel    &&
            SwitchInputController.Instance.RJoyConAccel.y                         <= 0.4f)
        {
            _progressRate =
                Mathf.Clamp(Mathf.Abs((_quaternion.eulerAngles.x - _RightJoyCon.eulerAngles.x) / _successRotation), 0f,
                            100f);

            if (_rightProgressRate <= _progressRate)
            {
                _rightProgressRate = _progressRate;
            }
        }

        SwitchInputController.Instance.LeftJoyConRotaion.GetQuaternion(ref _float4);
        _quaternion.Set(_float4.x, _float4.y, _float4.z, _float4.w);
        angle = Mathf.Abs(_quaternion.eulerAngles.x - _LeftJoyCon.eulerAngles.x);

        if (Mathf.Clamp(Mathf.Abs((Mathf.Repeat(angle + 180, 360) - 180)), 0, 30) >= _successRotation &&
            SwitchInputController.Instance.LJoyConAccel.y                         >= _successAccel    &&
            SwitchInputController.Instance.LJoyConAccel.y                         <= 0.4f)
        {
            _progressRate =
                Mathf.Clamp(Mathf.Abs((_quaternion.eulerAngles.x - _LeftJoyCon.eulerAngles.x) / _successRotation), 0f,
                            100f);

            if (_leftProgressRate <= _progressRate)
            {
                _leftProgressRate = _progressRate;
            }
        }

        //完全に開いた状態でtrue
        if (_leftProgressRate >= 90 && _rightProgressRate >= 90)
        {
            isOpened = true;

            _animator.SetTrigger("OpenDoar");
            _isOutline = false;

            if (isGameClearWhenOpened)
            {
                SoundManager.PlaySound(SoundDef.SetStar_SE);
            }

            SoundManager.PlaySound(SoundDef.DoorOpen_SE);

            // 左手の開くアニメーション
            UniTask leftHandOpenTask = PlayerHandController.TransitionHand(PlayerHandController.Hand.Left,
                                                                           PlayerHandController.HandPosition.DoorOpened,
                                                                           PlayerHandController.HandPosition.Idle);

            // 右手の開くアニメーション
            UniTask rightHandOpenTask = PlayerHandController.TransitionHand(PlayerHandController.Hand.Right,
                                                                            PlayerHandController.HandPosition.DoorOpened,
                                                                            PlayerHandController.HandPosition.Idle);

            // 同時再生
            await UniTask.WhenAll(leftHandOpenTask, rightHandOpenTask);

            // このドアを開くことでクリアとなるならステート変更
            if (isGameClearWhenOpened)
            {
                Gamemaneger.Instance.SetGameStateToResult(true);
            }
        }
    }

    /// <summary></summary>
    /// <param name="_defaultPosition">初期値の座標</param>
    /// <param name="_afterPosition">移動後の最終的な座標</param>
    /// <param name="proportion">現在の完了までの割合</param>
    Vector3 proportionFromPositionCalculation(Vector3 _defaultPosition, Vector3 _afterPosition, float proportion)
    {
        return Vector3.Lerp(_defaultPosition, _afterPosition, proportion / 100);
    }

    public void ShowOutline()
    {
        _rightDoar.layer = 9;
        _leftDoar.layer  = 9;
    }

    public void HideOutline()
    {
        _rightDoar.layer = 0;
        _leftDoar.layer  = 0;
    }
}
