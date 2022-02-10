using Cysharp.Threading.Tasks;
using UnityEngine;

public class Torch : MonoBehaviour,IActionable
{
    [SerializeField, Header("このオブジェクトに落下時に触れられた際に与えるダメージ量")]
    private int damageAmount;

    [SerializeField, Header("ゲーム開始時から落下状態とするか")]
    private bool isDroppedInDefault;

    private bool _isHoldByPlayer;

    private Camera    _cam;
    private Transform _holdPos;
    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;

    private int _defaultLayer;
    private int _ignorePlayerLayer;

    public bool     _isOutline  { get; private set; }
    public HandType RequireHand { get; private set; }
    public bool     isGrab      { get; private set; }

    [SerializeField, Header("支えなしverの松明")] private GameObject _torchGameObject;
    [SerializeField, Header("支えありverの松明")] private GameObject _supportTorchGameObject;
    void Start()
    {
        _cam          = Camera.main;

        isGrab       = true;
        _isOutline   = true;
        RequireHand  = HandType.One;
        _rigidbody   = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();

        _defaultLayer      = LayerMask.NameToLayer("Default");
        _ignorePlayerLayer = LayerMask.NameToLayer("IgnorePlayer");

        // ゲーム開始時から落ちてるものなら適宜動作切り替え
        if (isDroppedInDefault)
        {
            isGrab                 = false;
            _isOutline             = false;
            _boxCollider.isTrigger = false;
            _rigidbody.useGravity  = true;
        }
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
        // 持てる状態（まだプレイヤーに持たれていない）のときはダメージを与えない
        if (isGrab) return;

        var h = collision.collider.GetComponent<IHealth>();

        // プレイヤーならダメージを与える
        if (collision.collider.CompareTag("Player"))
        {
            h?.OnDamaged(damageAmount, gameObject);
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

        _torchGameObject.SetActive(true);
        _supportTorchGameObject.SetActive(false);

        _rigidbody.useGravity = false;
        _boxCollider.isTrigger = true;
        // 保持状態にする
        _isHoldByPlayer = true;

        gameObject.layer = _ignorePlayerLayer;

        // 保持位置を選択
        _holdPos = handType switch
        {
            HandType.Left  => PlayerManager.Instance.LeftHandTrf,
            HandType.Right => PlayerManager.Instance.RightHandTrf
        };

        PlayerHandController.TransitionHand(targetHand,
                                            PlayerHandController.HandPosition.Grab,
                                            PlayerHandController.HandPosition.SingleHold)
                            .Forget();
    }

    public void DeAction(HandType handType)
    {
        // 落としたら持てなくする
        isGrab                 = false;
        _isOutline             = false;
        _rigidbody.useGravity  = true;
        _boxCollider.isTrigger = false;
        _isHoldByPlayer        = false;

        gameObject.layer = _defaultLayer;

        PlayerHandController.Hand targetHand = handType switch
        {
            HandType.Left  => PlayerHandController.Hand.Left,
            HandType.Right => PlayerHandController.Hand.Right
        };

        PlayerHandController.SetPositionTo(PlayerHandController.HandPosition.Idle, targetHand, .15f).Forget();
    }

    public void ShowOutline()
    {
        _torchGameObject.layer = 9;
        _supportTorchGameObject.layer = 9;
    }

    public void HideOutline()
    {
        _torchGameObject.layer = _defaultLayer;
        _supportTorchGameObject.layer = _defaultLayer;
    }
}
