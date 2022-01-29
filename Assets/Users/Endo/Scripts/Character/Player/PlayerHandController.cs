using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerHandController : SingletonMonoBehaviour<PlayerHandController>
{
    [SerializeField, Header("左手オブジェクト")]
    private Transform leftHand;

    [SerializeField, Header("左手で掴む際の手座標の目的地")]
    private Transform leftHandGrabPos;

    [SerializeField, Header("左手で何かを持った際の定位置")]
    private Transform leftHandHoldPos;

    [SerializeField]
    private GameObject leftSpiderwebCheckHand;

    [SerializeField, Header("右手オブジェクト")]
    private Transform rightHand;

    [SerializeField, Header("右手で掴む際の手座標の目的地")]
    private Transform rightHandGrabPos;

    [SerializeField, Header("右手で何かを持った際の定位置")]
    private Transform rightHandHoldPos;

    [SerializeField]
    private GameObject rightSpiderwebCheckHand;

    [SerializeField]
    private Vector3 offset;

    private Camera _cam;

    private SwitchInputController _inputController;

    private Quaternion _leftRot;
    private Quaternion _rightRot;

    private Vector3    _leftIdlePos;
    private Vector3    _rightIdlePos;
    private Quaternion _leftIdleRot;
    private Quaternion _rightIdleRot;
    private Rigidbody  _leftRig;
    private Rigidbody  _rightRig;
    private Animator   _leftAnimator;
    private Animator   _rightAnimator;

    private bool _isAnimatingLHand;
    private bool _isAnimatingRHand;

    private nn.util.Float4 _npadRot = new nn.util.Float4();

    private CancellationTokenSource _lHandToken;
    private CancellationTokenSource _rHandToken;

    private static readonly int HandClose = Animator.StringToHash("handClose");
    private static readonly int HandOpen  = Animator.StringToHash("handOpen");

    public bool playerCaughtSpider;

    public enum Hand
    {
        Left,
        Right
    }

    public enum HandPosition
    {
        Idle,
        Grab,
        Hold
    }

    private async void Start()
    {
        _cam             = Camera.main;
        _inputController = SwitchInputController.Instance;
        _leftRig         = leftHand.GetComponent<Rigidbody>();
        _rightRig        = rightHand.GetComponent<Rigidbody>();
        _leftAnimator    = leftHand.GetComponent<Animator>();
        _rightAnimator   = rightHand.GetComponent<Animator>();
        _lHandToken      = new CancellationTokenSource();
        _rHandToken      = new CancellationTokenSource();

        _leftIdlePos  = leftHand.localPosition;
        _leftIdleRot  = leftHand.localRotation;
        _rightIdlePos = rightHand.localPosition;
        _rightIdleRot = rightHand.localRotation;

        if (leftSpiderwebCheckHand)
        {
            SetSpiderwebCheckHandActive(false);
        }
    }

    private void Update()
    {
        if (leftSpiderwebCheckHand && leftSpiderwebCheckHand.activeSelf)
        {
            UpdateHandRotation();
        }
    }

    /// <summary>
    /// 両手の回転をJoy-Con入力から計算・反映する
    /// </summary>
    private void UpdateHandRotation()
    {
        // 左手の回転
        _inputController.LeftJoyConRotaion.GetQuaternion(ref _npadRot);
        _leftRot.Set(_npadRot.x, _npadRot.z, _npadRot.y, -_npadRot.w);
        _leftRot *= Quaternion.AngleAxis(90, Vector3.right);
        _leftRot *= Quaternion.AngleAxis(180, Vector3.up);
        leftSpiderwebCheckHand.transform.rotation      = _leftRot;
        leftSpiderwebCheckHand.transform.localRotation = _leftRot; // ローカル回転にも代入してカメラの回転に追従させる

        // 右手の回転
        _inputController.RightJoyConRotaion.GetQuaternion(ref _npadRot);
        _rightRot.Set(_npadRot.x, _npadRot.z, _npadRot.y, -_npadRot.w);
        _rightRot                                       *= Quaternion.AngleAxis(90, Vector3.right);
        _rightRot                                       *= Quaternion.AngleAxis(180, Vector3.up);
        rightSpiderwebCheckHand.transform.rotation      =  _rightRot;
        rightSpiderwebCheckHand.transform.localRotation =  _rightRot;
    }

    public static void SetSpiderwebCheckHandActive(bool isActive)
    {
        Instance.leftSpiderwebCheckHand.SetActive(isActive);
        Instance.rightSpiderwebCheckHand.SetActive(isActive);
    }

    /// <summary>
    /// 両手の表示するかどうかを設定する
    /// </summary>
    /// <param name="isActive">表示するか</param>
    public static void SetHandActive(bool isActive)
    {
        Instance.leftHand.gameObject.SetActive(isActive);
        Instance.rightHand.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// 手をある状態からある状態までアニメーションを遷移させる
    /// </summary>
    /// <param name="handType">どちらの手をアニメーションさせるか</param>
    /// <param name="from">最初のアニメーション</param>
    /// <param name="to">次のアニメーション</param>
    public static async UniTask TransitionHand(Hand handType, HandPosition from, HandPosition to)
    {
        CancellationToken targetToken = handType switch
        {
            Hand.Left  => Instance._lHandToken.Token,
            Hand.Right => Instance._rHandToken.Token
        };

        switch (from)
        {
            case HandPosition.Idle:
            {
                await SetPositionTo(from, handType, .15f);

                break;
            }

            case HandPosition.Grab:
            {
                await SetPositionTo(from, handType, .35f);
                await UniTask.Delay(TimeSpan.FromSeconds(.1f), cancellationToken: targetToken)
                             .SuppressCancellationThrow();

                break;
            }

            case HandPosition.Hold:
            {
                await SetPositionTo(from, handType, .15f);

                break;
            }
        }

        switch (to)
        {
            case HandPosition.Idle:
            {
                await SetPositionTo(to, handType, .15f);

                break;
            }

            case HandPosition.Grab:
            {
                await SetPositionTo(to, handType, .35f);

                break;
            }

            case HandPosition.Hold:
            {
                await SetPositionTo(to, handType, .15f);

                break;
            }
        }
    }

    /// <summary>
    /// 手を移動させる
    /// </summary>
    /// <param name="handPos">移動先の状態</param>
    /// <param name="handType">どちらの手を移動させるか</param>
    /// <param name="time">何秒間で移動させるか</param>
    public static async UniTask SetPositionTo(HandPosition handPos, Hand handType, float time)
    {
        CancellationTokenSource targetToken = handType switch
        {
            Hand.Left  => Instance._lHandToken,
            Hand.Right => Instance._rHandToken
        };

        // 対象の手がアニメーション中ならキャンセル通知
        if (Instance._isAnimatingLHand && handType == Hand.Left ||
            Instance._isAnimatingRHand && handType == Hand.Right)
        {
            targetToken.Cancel();

            switch (handType)
            {
                case Hand.Left:
                {
                    Instance._lHandToken = new CancellationTokenSource();

                    break;
                }

                case Hand.Right:
                {
                    Instance._rHandToken = new CancellationTokenSource();

                    break;
                }
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        await SetPositionTo(handPos, handType, time, targetToken.Token).SuppressCancellationThrow();
    }

    /// <summary>
    /// 手を移動させる
    /// </summary>
    /// <param name="handPos">移動先の状態</param>
    /// <param name="handType">どちらの手を移動させるか</param>
    /// <param name="time">何秒間で移動させるか</param>
    /// <param name="token">CancellationToken</param>
    private static async UniTask SetPositionTo(HandPosition handPos, Hand handType, float time, CancellationToken token)
    {
        // 動かす対象の手オブジェクトを選択
        // TODO: 両手にも対応するため、配列などに変更する
        Transform targetTrf = handType switch
        {
            Hand.Left  => Instance.leftHand,
            Hand.Right => Instance.rightHand
        };

        // 移動後の位置・回転情報を選択
        HandTransform toMoveTrf = handPos switch
        {
            HandPosition.Idle => handType switch
            {
                Hand.Left  => new HandTransform(Instance._leftIdlePos, Instance._leftIdleRot),
                Hand.Right => new HandTransform(Instance._rightIdlePos, Instance._rightIdleRot)
            },
            HandPosition.Grab => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandGrabPos),
                Hand.Right => new HandTransform(Instance.rightHandGrabPos)
            },
            HandPosition.Hold => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandHoldPos),
                Hand.Right => new HandTransform(Instance.rightHandHoldPos)
            }
        };

        // アニメーションする場合の対象の手のAnimatorを選択
        Animator targetAnim = handType switch
        {
            Hand.Left  => Instance._leftAnimator,
            Hand.Right => Instance._rightAnimator,
        };

        // アニメーション状態設定
        SetHandAnimating(true);

        float timeElapsed     = 0;
        float transitionRatio = 0;
        var   fromHandTrf     = new HandTransform(targetTrf);
        bool  isAnimated      = false;

        // 移動処理
        while (transitionRatio < 1)
        {
            // キャンセルされてたらアニメーション停止
            if (token.IsCancellationRequested)
            {
                SetHandAnimating(false);
                token.ThrowIfCancellationRequested();

                return;
            }

            // 割合計算
            transitionRatio = timeElapsed / time;

            // 座標設定
            targetTrf.localPosition = Vector3.Slerp(fromHandTrf.Position, toMoveTrf.Position, transitionRatio);

            // 回転設定
            targetTrf.localRotation = Quaternion.Slerp(fromHandTrf.Rotation, toMoveTrf.Rotation, transitionRatio);

            // 合間に掴むアニメーションを再生する
            if (!isAnimated && transitionRatio >= .9f)
            {
                isAnimated = true;

                switch (handPos)
                {
                    case HandPosition.Idle:
                    {
                        targetAnim.SetTrigger(HandOpen);

                        break;
                    }

                    case HandPosition.Grab:
                    {
                        targetAnim.SetTrigger(HandClose);

                        break;
                    }
                }
            }

            await UniTask.Yield(PlayerLoopTiming.Update);

            timeElapsed += Time.deltaTime;
        }

        SetHandAnimating(false);

        void SetHandAnimating(bool toSet)
        {
            if (handType == Hand.Left)
            {
                Instance._isAnimatingLHand = toSet;
            }
            else if (handType == Hand.Right)
            {
                Instance._isAnimatingRHand = toSet;
            }
        }
    }

    private class HandTransform
    {
        public Vector3    Position;
        public Quaternion Rotation;

        public HandTransform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public HandTransform(Transform target)
        {
            Position = target.localPosition;
            Rotation = target.localRotation;
        }
    }
}
