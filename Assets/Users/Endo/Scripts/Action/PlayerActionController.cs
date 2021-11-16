using System.Collections.Generic;
using UnityEngine;
using UniRx;

[RequireComponent(typeof(SphereCollider))]
public class PlayerActionController : MonoBehaviour
{
    [SerializeField, Header("アクション可能範囲の半径"), Range(.1f, 5)]
    private float actionRange;

    [SerializeField, Header("デバッグ表示をするか")]
    private bool isDebug;

    private readonly Dictionary<Collider, IActionable> _inRangeObjects       = new Dictionary<Collider, IActionable>();
    private readonly List<Collider>                    _sortedInRangeObjects = new List<Collider>();

    private Camera         _cam;
    private SphereCollider _selfCollider;

    /// <summary>視点を合わせているオブジェクト</summary>
    private IActionable _focusObject;

    /// <summary>左手でアクション中のオブジェクト</summary>
    private IActionable _leftHoldObj;

    /// <summary>右手でアクション中のオブジェクト</summary>
    private IActionable _rightHoldObj;

    /// <summary>
    /// ZR,ZLの同時押しの判定用
    /// 片方がGetButtonDownされたらtrueになる
    /// GetButtonUpが呼ばれたらfalse
    /// </summary>
    private bool pushing_simultaneous_R = false;

    private bool pushing_simultaneous_L = false;

    /// <summary>左手になにか持っているか</summary>
    public bool IsHoldInLeft => _leftHoldObj != null;

    /// <summary>右手になにか持っているか</summary>
    public bool IsHoldInRight => _rightHoldObj != null;

    private void Start()
    {
        _cam                 = Camera.main;
        _selfCollider        = GetComponent<SphereCollider>();
        _selfCollider.radius = actionRange;

        SwitchInputController.Instance.OnClickGrabButtonSubject.Subscribe(_GrabButton =>
        {
            SwitchInputController.Status _ZRStatus = _GrabButton.ZR.Status;
            SwitchInputController.Status _ZLStatus = _GrabButton.ZL.Status;

            pushing_simultaneous_R = _ZRStatus == SwitchInputController.Status.GetButtonDown;
            pushing_simultaneous_L = _ZLStatus == SwitchInputController.Status.GetButtonDown;

            // 視点の先のオブジェクトを、要求の持ち方に応じて処理
            switch (_focusObject?.RequireHand)
            {
                case HandType.Undefined:
                {
                    Debug.LogError($"持ち方が定義されていません: {_focusObject}");

                    break;
                }

                case HandType.Left:
                {
                    // 左トリガーのみ押下および左手未所持のときに処理
                    if (!(pushing_simultaneous_L && pushing_simultaneous_R && _leftHoldObj == null)) break;

                    // 左手に持たせてアクション実行
                    _leftHoldObj = _focusObject;
                    _leftHoldObj?.Action();

                    break;
                }

                case HandType.Right:
                {
                    // 右トリガーのみ押下および右手未所持のときに処理
                    if (!(pushing_simultaneous_R && !pushing_simultaneous_L && _rightHoldObj == null)) break;

                    // 右手に持たせてアクション実行
                    _rightHoldObj = _focusObject;
                    _rightHoldObj?.Action();

                    break;
                }

                case HandType.One:
                {
                    // トリガー押下時のみ処理
                    if (!pushing_simultaneous_L && !pushing_simultaneous_R) break;

                    if (pushing_simultaneous_L && !pushing_simultaneous_R && _leftHoldObj == null)
                    {
                        _leftHoldObj = _focusObject;
                        _leftHoldObj?.Action();
                    }
                    else if (pushing_simultaneous_R && !pushing_simultaneous_L && _rightHoldObj == null)
                    {
                        _rightHoldObj = _focusObject;
                        _rightHoldObj?.Action();
                    }

                    break;
                }

                case HandType.Both:
                {
                    // 片方の入力がされた状態でGetButtonDownが呼び出されないのでboolで対応
                    if (pushing_simultaneous_L && pushing_simultaneous_R                              ||
                        pushing_simultaneous_L && _ZRStatus == SwitchInputController.Status.GetButton ||
                        pushing_simultaneous_R && _ZLStatus == SwitchInputController.Status.GetButton)
                    {
                        // 両手に持たせてアクション実行
                        _leftHoldObj = _rightHoldObj = _focusObject;
                        _focusObject?.Action();
                    }

                    break;
                }
            }

            // トリガー開放時にオブジェクトを持ってるなら開放処理実行
            if (_ZLStatus == SwitchInputController.Status.GetButtonUp)
            {
                _leftHoldObj?.DeAction();
                _leftHoldObj = null;
            }

            if (_ZRStatus == SwitchInputController.Status.GetButtonUp)
            {
                _rightHoldObj?.DeAction();
                _rightHoldObj = null;
            }
        });
    }

    private void Update()
    {
        // 視点の先にアクション可能なオブジェクトがあるか確認
        // TODO: パフォーマンス的に、視点とプレイヤーが移動していなければ実行させないようにしたい
        if (_inRangeObjects.Count != 0)
        {
            CheckActionableFocus();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!isDebug) return;
        if (_cam == null) return;

        // 向いている方向の表示
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _cam.transform.forward * actionRange);
    }
#endif

    /// <summary>
    /// 視点の先にアクション可能なオブジェクトが存在するかを確認する。存在すればそのオブジェクトを記憶。
    /// </summary>
    /// <returns>アクション可能なオブジェクトが存在するか</returns>
    private bool CheckActionableFocus()
    {
        var ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, actionRange))
        {
            var act = hit.collider.GetComponent<IActionable>();

            if (act == null) return false;

            _focusObject = act;

            return true;
        }

        _focusObject = null;

        return false;
    }

    /// <summary>
    /// 最も近いアクション可能オブジェクトをアクションする
    /// </summary>
    private void DoActionClosest()
    {
        // アクション対象が存在するときのみ処理
        if (_sortedInRangeObjects.Count == 0) return;

        // 範囲内のオブジェクトを近い順にソート
        _sortedInRangeObjects.Sort((a, b) =>
        {
            float aDis = Vector3.Distance(a.transform.position, transform.position);
            float bDis = Vector3.Distance(b.transform.position, transform.position);

            return (aDis - bDis) < 0 ? -1 : 1;
        });

        // 最も近いオブジェクトをアクションさせる
        foreach (KeyValuePair<Collider, IActionable> obj in _inRangeObjects)
        {
            if (obj.Key != _sortedInRangeObjects[0]) continue;

            obj.Value.Action();

            break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponent<IActionable>();

        // アクション対象でなければ終了
        if (target == null) return;

        if (!_inRangeObjects.ContainsKey(other))
        {
            _inRangeObjects.Add(other, target);
        }

        _sortedInRangeObjects.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Collider target = null;

        // 範囲内リストのオブジェクトなら削除する
        foreach (KeyValuePair<Collider, IActionable> obj in _inRangeObjects)
        {
            if (other != obj.Key) continue;

            target = obj.Key;
        }

        if (target == null) return;

        _inRangeObjects.Remove(other);
        _sortedInRangeObjects.Remove(other);
    }
}
