using Cysharp.Threading.Tasks;
using UnityEngine;

public class Rock : MonoBehaviour, IActionable
{
    //所持しているかどうか
    private bool _isHold;
    //rigidbody
    private Rigidbody _rg;
    //playerカメラ
    private Camera _cam;

    private bool _isDeActioned;

    private int _defaultLayer;
    private int _ignorePlayerLayer;

    //コピペ
    public HandType RequireHand { get; private set; }
    public bool isGrab { get; private set; }
    public bool _isOutline { get; private set; }

    private void Awake()
    {
        _rg = GetComponent<Rigidbody>();
        _cam = Camera.main;
    }

    void Start()
    {
        //変数の初期化
        _isHold = false;
        isGrab = true;
        _isOutline = true;
        RequireHand = HandType.Both;

        _defaultLayer      = LayerMask.NameToLayer("Default");
        _ignorePlayerLayer = LayerMask.NameToLayer("IgnorePlayer");
    }
    private void LateUpdate()
    {
        if (_isHold)
        {
            var targetpos = PlayerManager.Instance.MidHandTrf;
            transform.position = targetpos.position;
            transform.localRotation = _cam.transform.rotation;
        }
    }
    public void Action(HandType handType)
    {
        _isDeActioned = false;
        _isOutline    = false;

        //保持状態の更新
        _isHold                   = true;
        _rg.useGravity            = false;
        _rg.constraints           = RigidbodyConstraints.FreezeRotation;
        Player.Instance.SpeedBuff = 0.5f;

        // プレイヤーに干渉しないようにするためレイヤー変更
        gameObject.layer = _ignorePlayerLayer;

        PlayerHandController.SetPositionTo(PlayerHandController.HandPosition.BothHold, PlayerHandController.Hand.Left)
                            .Forget();

        PlayerHandController.SetPositionTo(PlayerHandController.HandPosition.BothHold, PlayerHandController.Hand.Right)
                            .Forget();
    }
    public void DeAction(HandType handType)
    {
        // ZL/ZRが離すたびに呼ばれ、連続するとSetPositionToでキャンセル処理が走るため、その防止
        if (_isDeActioned) return;

        _isDeActioned = true;
        _isOutline    = true;

        //保持状態の更新
        _isHold                   = false;
        _rg.useGravity            = true;
        _rg.constraints           = RigidbodyConstraints.None;
        Player.Instance.SpeedBuff = 1f;

        gameObject.layer = _defaultLayer;

        PlayerHandController.SetPositionTo(PlayerHandController.HandPosition.Idle, PlayerHandController.Hand.Left)
                            .Forget();

        PlayerHandController.SetPositionTo(PlayerHandController.HandPosition.Idle, PlayerHandController.Hand.Right)
                            .Forget();
    }
    public void ShowOutline()
    {
        gameObject.layer = 9;
    }

    public void HideOutline()
    {
        gameObject.layer = _defaultLayer;
    }
}
