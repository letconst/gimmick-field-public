using System.Collections.Generic;
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

    [SerializeField, Header("左手で両手用の何かを持った際の定位置")]
    private Transform leftHandBothHoldPos;

    [SerializeField, Header("左手で何かを投げた際の最終位置")]
    private Transform leftHandThrowPos;

    [SerializeField, Header("左手で宝箱を掴んだ際の位置")]
    private Transform leftHandTreasureGrabPos;

    [SerializeField, Header("左手で宝箱を開いた際の最終位置")]
    private Transform leftHandTreasureOpenedPos;

    [SerializeField, Header("左手で扉を掴んだ際の位置")]
    private Transform leftHandDoorGrabPos;

    [SerializeField, Header("左手で扉を開いた際の最終位置")]
    private Transform leftHandDoorOpenedPos;

    [SerializeField]
    private GameObject leftSpiderwebCheckHand;

    [SerializeField, Header("右手オブジェクト")]
    private Transform rightHand;

    [SerializeField, Header("右手で掴む際の手座標の目的地")]
    private Transform rightHandGrabPos;

    [SerializeField, Header("右手で何かを持った際の定位置")]
    private Transform rightHandHoldPos;

    [SerializeField, Header("右手で両手用の何かを持った際の定位置")]
    private Transform rightHandBothHoldPos;

    [SerializeField, Header("右手で何かを投げた際の最終位置")]
    private Transform rightHandThrowPos;

    [SerializeField, Header("右手で宝箱を掴んだ際の位置")]
    private Transform rightHandTreasureGrabPos;

    [SerializeField, Header("右手で宝箱を開いた際の最終位置")]
    private Transform rightHandTreasureOpenedPos;

    [SerializeField, Header("右手で扉を掴んだ際の位置")]
    private Transform rightHandDoorGrabPos;

    [SerializeField, Header("右手で扉を開いた際の最終位置")]
    private Transform rightHandDoorOpenedPos;

    [SerializeField]
    private GameObject rightSpiderwebCheckHand;

    [SerializeField]
    private Vector3 offset;

    [SerializeField, Header("各モーションの設定。必ずHandPositionのenum順通り設定してください")]
    private List<HandOptions> handOptions;

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
    private Animator   _leftAnimatorForSpiderWeb;
    private Animator   _rightAnimatorForSpiderWeb;

    private bool _isAnimatingLHand;
    private bool _isAnimatingRHand;

    private nn.util.Float4 _npadRot = new nn.util.Float4();

    private CancellationTokenSource _lHandToken;
    private CancellationTokenSource _rHandToken;

    public static readonly int HandClose  = Animator.StringToHash("handClose");
    public static readonly int HandOpen   = Animator.StringToHash("handOpen");
    public static readonly int HandOpened = Animator.StringToHash("handOpened");

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
        SingleHold,
        BothHold,
        Throw,
        DoorGrab,
        DoorOpened,
        TreasureBoxGrab,
        TreasureBoxOpened
    }

    public enum HandHoldOption
    {
        None,
        Open,
        Close
    }

    [SerializeField]
    private BoxCollider R_O,
                        R_I,
                        L_O,
                        L_I,
                        CreateStarStonePillarChecker;

    private async void Start()
    {
        _cam                       = Camera.main;
        _inputController           = SwitchInputController.Instance;
        _leftRig                   = leftHand.GetComponent<Rigidbody>();
        _rightRig                  = rightHand.GetComponent<Rigidbody>();
        _leftAnimator              = leftHand.GetComponent<Animator>();
        _rightAnimator             = rightHand.GetComponent<Animator>();
        _leftAnimatorForSpiderWeb  = leftSpiderwebCheckHand.GetComponent<Animator>();
        _rightAnimatorForSpiderWeb = rightSpiderwebCheckHand.GetComponent<Animator>();
        _lHandToken                = new CancellationTokenSource();
        _rHandToken                = new CancellationTokenSource();

        _leftIdlePos  = leftHand.localPosition;
        _leftIdleRot  = leftHand.localRotation;
        _rightIdlePos = rightHand.localPosition;
        _rightIdleRot = rightHand.localRotation;

        SetSpiderwebCheckHandActive(false);

        if (leftSpiderwebCheckHand)
        {
            SetSpiderwebCheckHandActive(false);
        }

        string[] handPositions = System.Enum.GetNames(typeof(HandPosition));

        // handOptionsとHandPositionの要素数は少なくとも一致していなければいけないため
        // 不一致なら警告
        if (handOptions.Count != handPositions.Length)
        {
            Debug.LogWarning("handOptionsとHandPositionの要素数が一致しません");
        }
    }

    private void Update()
    {
        UpdateHandRotation();
    }

    /// <summary>
    /// 両手の回転をJoy-Con入力から計算・反映する
    /// </summary>
    private void UpdateHandRotation()
    {
        if (leftSpiderwebCheckHand.activeSelf)
        {
            // 左手の回転
            _inputController.LeftJoyConRotaion.GetQuaternion(ref _npadRot);
            _leftRot.Set(_npadRot.x, _npadRot.z, _npadRot.y, -_npadRot.w);
            _leftRot                                       *= Quaternion.AngleAxis(90, Vector3.right);
            _leftRot                                       *= Quaternion.AngleAxis(180, Vector3.up);
            leftSpiderwebCheckHand.transform.rotation      =  _leftRot;
            leftSpiderwebCheckHand.transform.localRotation =  _leftRot; // ローカル回転にも代入してカメラの回転に追従させる
        }

        if (rightSpiderwebCheckHand.activeSelf)
        {
            // 右手の回転
            _inputController.RightJoyConRotaion.GetQuaternion(ref _npadRot);
            _rightRot.Set(_npadRot.x, _npadRot.z, _npadRot.y, -_npadRot.w);
            _rightRot                                       *= Quaternion.AngleAxis(90, Vector3.right);
            _rightRot                                       *= Quaternion.AngleAxis(180, Vector3.up);
            rightSpiderwebCheckHand.transform.rotation      =  _rightRot;
            rightSpiderwebCheckHand.transform.localRotation =  _rightRot;
        }
    }

    public static void SetSpiderwebCheckHandActive(bool isActive, HandType handType = HandType.Both,
                                                   bool handOpened = true)
    {
        switch (handType)
        {
            case HandType.Both:
            {
                Instance.leftSpiderwebCheckHand.SetActive(isActive);
                Instance.rightSpiderwebCheckHand.SetActive(isActive);
                // Instance.SetActiveChild(Instance.leftSpiderwebCheckHand, isActive);
                // Instance.SetActiveChild(Instance.rightSpiderwebCheckHand, isActive);
                Instance._leftAnimatorForSpiderWeb.SetBool(HandOpened, handOpened);
                Instance._rightAnimatorForSpiderWeb.SetBool(HandOpened, handOpened);
                Instance.CreateStarStonePillarChecker.enabled = isActive;
                Instance.R_O.enabled                          = isActive;
                Instance.R_I.enabled                          = isActive;
                Instance.L_O.enabled                          = isActive;
                Instance.L_I.enabled                          = isActive;
            }

                break;

            case HandType.Left:
            {
                Instance.leftSpiderwebCheckHand.SetActive(isActive);
                // Instance.SetActiveChild(Instance.leftSpiderwebCheckHand, isActive);
                Instance.CreateStarStonePillarChecker.enabled = isActive;
                Instance._leftAnimatorForSpiderWeb.SetBool(HandOpened, handOpened);
                Instance.L_O.enabled = isActive;
                Instance.L_I.enabled = isActive;
            }

                break;

            case HandType.Right:
            {
                Instance.rightSpiderwebCheckHand.SetActive(isActive);
                // Instance.SetActiveChild(Instance.rightSpiderwebCheckHand, isActive);
                Instance.CreateStarStonePillarChecker.enabled = isActive;
                Instance._rightAnimatorForSpiderWeb.SetBool(HandOpened, handOpened);
                Instance.R_O.enabled = isActive;
                Instance.R_I.enabled = isActive;
            }

                break;
        }
    }

    bool SetActiveChild(GameObject parent, bool isActive)
    {
        int childCount = parent.transform.childCount;

        if (childCount <= 0)
        {
            return false;
        }

        for (int i = 0; i < childCount; i++)
        {
            parent.transform.GetChild(i).gameObject.SetActive(isActive);
            SetActiveChild(parent.transform.GetChild(i).gameObject, isActive);
        }

        return true;
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
        CancellationToken targetCts = handType switch
        {
            Hand.Left  => Instance._lHandToken.Token,
            Hand.Right => Instance._rHandToken.Token
        };

        float fromTransitionSec = GetDefaultTransitionSeconds(from);
        float toTransitionSec   = GetDefaultTransitionSeconds(to);

        // 最初のアニメーション再生
        await SetPositionTo(from, handType, fromTransitionSec);

        // 合間に行いたい処理
        switch (from)
        {
            case HandPosition.Grab:
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(.1f), cancellationToken: targetCts)
                             .SuppressCancellationThrow();

                break;
            }
        }

        // 次のアニメーション再生
        await SetPositionTo(to, handType, toTransitionSec);
    }

    /// <summary>
    /// 手を移動させる（既定値のアニメーション時間が使用される）
    /// </summary>
    /// <param name="handPos">移動先の状態</param>
    /// <param name="handType">どちらの手を移動させるか</param>
    public static async UniTask SetPositionTo(HandPosition handPos, Hand handType)
    {
        float transitionSec = GetDefaultTransitionSeconds(handPos);

        await SetPositionTo(handPos, handType, transitionSec);
    }

    /// <summary>
    /// 手を移動させる（アニメーション時間を任意指定可能）
    /// </summary>
    /// <param name="handPos">移動先の状態</param>
    /// <param name="handType">どちらの手を移動させるか</param>
    /// <param name="transitionSeconds">何秒間で移動させるか</param>
    public static async UniTask SetPositionTo(HandPosition handPos, Hand handType, float transitionSeconds)
    {
        CancellationTokenSource targetCts = handType switch
        {
            Hand.Left  => Instance._lHandToken,
            Hand.Right => Instance._rHandToken
        };

        // 対象の手がアニメーション中ならキャンセル通知
        // if (Instance._isAnimatingLHand && handType == Hand.Left ||
        //     Instance._isAnimatingRHand && handType == Hand.Right)
        // {
        //     targetCts.Cancel();
        //
        //     switch (handType)
        //     {
        //         case Hand.Left:
        //         {
        //             Instance._lHandToken = new CancellationTokenSource();
        //
        //             break;
        //         }
        //
        //         case Hand.Right:
        //         {
        //             Instance._rHandToken = new CancellationTokenSource();
        //
        //             break;
        //         }
        //     }
        //
        //     await UniTask.Yield(PlayerLoopTiming.Update);
        // }

        await SetPositionTo(handPos, handType, transitionSeconds, targetCts.Token).SuppressCancellationThrow();
    }

    /// <summary>
    /// 手を移動させる
    /// </summary>
    /// <param name="handPos">移動先の状態</param>
    /// <param name="handType">どちらの手を移動させるか</param>
    /// <param name="transitionSeconds">何秒間で移動させるか</param>
    /// <param name="cts">CancellationToken</param>
    private static async UniTask SetPositionTo(HandPosition      handPos, Hand handType, float transitionSeconds,
                                               CancellationToken cts)
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
            HandPosition.SingleHold => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandHoldPos),
                Hand.Right => new HandTransform(Instance.rightHandHoldPos)
            },
            HandPosition.BothHold => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandBothHoldPos),
                Hand.Right => new HandTransform(Instance.rightHandBothHoldPos)
            },
            HandPosition.Throw => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandThrowPos),
                Hand.Right => new HandTransform(Instance.rightHandThrowPos)
            },
            HandPosition.DoorGrab => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandDoorGrabPos),
                Hand.Right => new HandTransform(Instance.rightHandDoorGrabPos)
            },
            HandPosition.DoorOpened => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandDoorOpenedPos),
                Hand.Right => new HandTransform(Instance.rightHandDoorOpenedPos)
            },
            HandPosition.TreasureBoxGrab => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandTreasureGrabPos),
                Hand.Right => new HandTransform(Instance.rightHandTreasureGrabPos)
            },
            HandPosition.TreasureBoxOpened => handType switch
            {
                Hand.Left  => new HandTransform(Instance.leftHandTreasureOpenedPos),
                Hand.Right => new HandTransform(Instance.rightHandTreasureOpenedPos)
            }
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
            // if (cts.IsCancellationRequested)
            // {
            //     SetHandAnimating(false);
            //     cts.ThrowIfCancellationRequested();
            //
            //     return;
            // }

            // 割合計算
            transitionRatio = timeElapsed / transitionSeconds;

            // 座標設定
            targetTrf.localPosition = Vector3.Slerp(fromHandTrf.Position, toMoveTrf.Position, transitionRatio);

            // 回転設定
            targetTrf.localRotation = Quaternion.Slerp(fromHandTrf.Rotation, toMoveTrf.Rotation, transitionRatio);

            // 合間に掴むアニメーションを再生する
            if (!isAnimated)
            {
                isAnimated = PlayHandHoldAnimationAtRatio(handType, handPos, transitionRatio);
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

    /// <summary>
    /// 手のホールドアニメーションを、特定のタイミングで再生する。タイミング設定はHandOptionsで行う
    /// </summary>
    /// <param name="handType">どちらの手を移動させるか</param>
    /// <param name="handPos">移動先の状態</param>
    /// <param name="currentRatio">現在の移動モーション再生比率 (0-1)</param>
    /// <returns>再生が完了できたか</returns>
    private static bool PlayHandHoldAnimationAtRatio(Hand handType, HandPosition handPos, float currentRatio)
    {
        // handPosに対応するHandOptionsを取得
        HandOptions options = Instance.handOptions[(int) handPos];

        if (options == null) return false;

        // アニメーションを再生するタイミングに達してなければ終了
        if (currentRatio < options.HoldTransitionRatio) return false;

        // アニメーションする場合の対象の手のAnimatorを選択
        Animator targetAnim = handType switch
        {
            Hand.Left  => Instance._leftAnimator,
            Hand.Right => Instance._rightAnimator,
        };

        // ホールドアニメーション再生
        switch (options.HandHoldOption)
        {
            case HandHoldOption.Open:
                targetAnim.SetBool(HandOpened, true);

                break;

            case HandHoldOption.Close:
                targetAnim.SetBool(HandOpened, false);

                break;
        }

        return true;
    }

    /// <summary>
    /// 指定の手の位置への既定アニメーション移動秒数を取得する
    /// </summary>
    /// <param name="handPos">手の位置</param>
    /// <returns>既定のアニメーション秒数</returns>
    private static float GetDefaultTransitionSeconds(HandPosition handPos)
    {
        HandOptions options = Instance.handOptions[(int) handPos];

        if (options == null) return -1;

        return options.TransitionSeconds;
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

    [System.Serializable]
    private class HandOptions
    {
        [Header("どの動きの手の設定をするか")]
        public HandPosition HandPosition;

        [Header("どのホールドアニメーションを再生するか。Noneで再生しない")]
        public HandHoldOption HandHoldOption;

        [Header("ホールドアニメーションを再生するタイミングの比率"), Range(0, 1)]
        public float HoldTransitionRatio;

        [Header("アニメーションを再生する秒数"), Min(.1f)]
        public float TransitionSeconds;
    }
}
