using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Star : MonoBehaviour, IActionable
{
    [SerializeField, Header("投げた際の速度の感度"), Range(.1f, 50)]
    private float throwSensitivity;

    [SerializeField, Header("このオブジェクトが与えるダメージ量")]
    private int damageAmount;

    [SerializeField, Header("このオブジェクトがダメージを与えるために必要な速度 (m/s)")]
    private float takeDamageSpeedThreshold;

    private bool _isHoldByPlayer;

    private Rigidbody _selfRig;
    private Collider  _selfCollider;

    private Camera    _cam;
    private Transform _holdPos;

    private int _defaultLayer;
    private int _ignorePlayerLayer;

    public bool     _isOutline  { get; private set; }
    public HandType RequireHand { get; private set; }
    public bool     isGrab      { get; private set; }

    private void Start()
    {
        _selfRig      = GetComponent<Rigidbody>();
        _selfCollider = GetComponent<Collider>();
        _cam          = Camera.main;

        isGrab      = true;
        _isOutline  = true;
        RequireHand = HandType.One;

        _defaultLayer      = LayerMask.NameToLayer("Default");
        _ignorePlayerLayer = LayerMask.NameToLayer("IgnorePlayer");
    }

    private void LateUpdate()
    {
        if (_isHoldByPlayer)
        {
            transform.position      = _holdPos.position;
            transform.localRotation = _cam.transform.rotation;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var h = collision.collider.GetComponent<IHealth>();

        // 一定の加速度以上のとき
        if (_selfRig.velocity.magnitude > takeDamageSpeedThreshold)
        {
            // 体力を持つオブジェクトに衝突した場合は攻撃を通知
            h?.OnDamaged(damageAmount, gameObject);

            SpawnParticle();

            SoundManager.PlaySound(SoundDef.StoneCollision_SE, position: transform.position);
        }
    }

    public async void Action(HandType handType)
    {
        // アニメーションさせる手を選択
        PlayerHandController.Hand targetHand = handType switch
        {
            HandType.Left  => PlayerHandController.Hand.Left,
            HandType.Right => PlayerHandController.Hand.Right
        };

        if (!isGrab)
        {
            // 持てない状態の場合はスカアニメーションを再生して終了
            PlayerHandController.TransitionHand(targetHand,
                                                PlayerHandController.HandPosition.Grab,
                                                PlayerHandController.HandPosition.Idle)
                                .Forget();

            return;
        }

        // 保持状態にする
        _isHoldByPlayer = true;
        // 重力無効化
        _selfRig.useGravity   = false;
        _selfRig.constraints  = RigidbodyConstraints.FreezeRotation;
        _selfCollider.enabled = false;

        gameObject.layer = _ignorePlayerLayer;

        // 保持位置を選択
        _holdPos = handType switch
        {
            HandType.Left  => PlayerManager.Instance.LeftHandTrf,
            HandType.Right => PlayerManager.Instance.RightHandTrf
        };

        await PlayerHandController.TransitionHand(targetHand,
                                                  PlayerHandController.HandPosition.Grab,
                                                  PlayerHandController.HandPosition.SingleHold);
    }

    public void DeAction(HandType handType)
    {
        if (!isGrab) return;

        _isHoldByPlayer       = false;
        _selfRig.useGravity   = true;
        _selfRig.constraints  = RigidbodyConstraints.None;
        _selfCollider.enabled = true;

        gameObject.layer = _defaultLayer;

        // 離された手に対応する加速度を取得
        nn.util.Float3 targetAcc = handType switch
        {
            HandType.Left  => SwitchInputController.Instance.LeftJoyConRotaion.acceleration,
            HandType.Right => SwitchInputController.Instance.RightJoyConRotaion.acceleration
        };

        Vector3 dir          = _cam.transform.forward;
        float   acceleration = new Vector3(targetAcc.x, targetAcc.y, targetAcc.z).magnitude;

        // 持ってる石を、Joy-Conを振った加速度で飛ばす
        _selfRig.AddForce(dir * acceleration * throwSensitivity, ForceMode.VelocityChange);

        // アニメーションさせる手を選択
        PlayerHandController.Hand targetHand = handType switch
        {
            HandType.Left  => PlayerHandController.Hand.Left,
            HandType.Right => PlayerHandController.Hand.Right
        };

        PlayerHandController.TransitionHand(targetHand,
                                            PlayerHandController.HandPosition.Throw,
                                            PlayerHandController.HandPosition.Idle);
    }

    public void ShowOutline()
    {
        gameObject.layer = 9;
    }

    public void HideOutline()
    {
        gameObject.layer = _defaultLayer;
    }

    public void SetStarOnPosition()
    {
        if (_isHoldByPlayer) return;

        isGrab                                                = false;
        _isOutline                                            = false;
        this.gameObject.transform.rotation                    = new Quaternion(0, 0, 0, 0);
        this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }

    private async void SpawnParticle()
    {
        ParticlePlayer particle = SceneControllerBase.Instance.dustParticlePool.Rent();
        particle.transform.position = transform.position;
        particle.PlayParticle();

        // パーティクルが停止するまで待機
        while (!particle.selfParticle.isStopped)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        MainGameController.Instance.dustParticlePool.Return(particle);
    }
}
