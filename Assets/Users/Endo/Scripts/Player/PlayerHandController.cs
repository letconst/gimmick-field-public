using UnityEngine;
using UniRx;

public class PlayerHandController : MonoBehaviour
{
    [SerializeField]
    private Transform leftHand;

    [SerializeField]
    private Transform rightHand;

    [SerializeField]
    private Vector3 offset;

    private Camera _cam;

    private SwitchInputController _inputController;

    private Quaternion _leftRot;
    private Quaternion _rightRot;

    private Rigidbody _leftRig;
    private Rigidbody _rightRig;
    private Animator  _leftAnimator;
    private Animator  _rightAnimator;

    private nn.util.Float4 _npadRot = new nn.util.Float4();

    private static readonly int HandClose = Animator.StringToHash("handClose");
    private static readonly int HandOpen  = Animator.StringToHash("handOpen");

    private void Start()
    {
        _cam             = Camera.main;
        _inputController = SwitchInputController.Instance;
        _leftRig         = leftHand.GetComponent<Rigidbody>();
        _rightRig        = rightHand.GetComponent<Rigidbody>();
        _leftAnimator    = leftHand.GetComponent<Animator>();
        _rightAnimator   = rightHand.GetComponent<Animator>();

        _inputController.OnClickGrabButtonSubject.Subscribe(trigger =>
        {
            // 左手のアニメーション処理
            switch (trigger.ZL.Status)
            {
                case SwitchInputController.Status.GetButtonDown:
                {
                    _leftAnimator.SetTrigger(HandClose);

                    break;
                }

                case SwitchInputController.Status.GetButtonUp:
                {
                    _leftAnimator.SetTrigger(HandOpen);

                    break;
                }
            }

            // 右手のアニメーション処理
            switch (trigger.ZR.Status)
            {
                case SwitchInputController.Status.GetButtonDown:
                {
                    _rightAnimator.SetTrigger(HandClose);

                    break;
                }

                case SwitchInputController.Status.GetButtonUp:
                {
                    _rightAnimator.SetTrigger(HandOpen);

                    break;
                }
            }
        });
    }

    private void Update()
    {
        UpdateHandRotation();
    }

    // private void FixedUpdate()
    // {
    //     UpdateHandPosition();
    // }

    /// <summary>
    /// 両手の回転をJoy-Con入力から計算・反映する
    /// </summary>
    private void UpdateHandRotation()
    {
        // 左手の回転
        _inputController.LeftJoyConRotaion.GetQuaternion(ref _npadRot);
        _leftRot.Set(_npadRot.x, _npadRot.z, _npadRot.y, -_npadRot.w);
        _leftRot          *= Quaternion.Euler(90, 90, 90);
        _leftRot          *= Quaternion.Euler(0, 180, 0);
        leftHand.rotation =  _leftRot;

        // 右手の回転
        _inputController.RightJoyConRotaion.GetQuaternion(ref _npadRot);
        _rightRot.Set(_npadRot.x, _npadRot.z, _npadRot.y, -_npadRot.w);
        _rightRot          *= Quaternion.Euler(90, 90, 90);
        _rightRot          *= Quaternion.Euler(0, 180, 0);
        rightHand.rotation =  _rightRot;
    }
}
