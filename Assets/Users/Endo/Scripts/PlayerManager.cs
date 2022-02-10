using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    [FormerlySerializedAs("playerHandTrf")]
    [SerializeField]
    private Transform leftHandTrf;

    [SerializeField]
    private Transform rightHandTrf;

    [SerializeField]
    private Transform midHandTrf;

    public Transform LeftHandTrf  => leftHandTrf;
    public Transform RightHandTrf => rightHandTrf;

    public Transform MidHandTrf => midHandTrf;

    private IActionable _holdItem;

    /// <summary>
    /// プレイヤーが持っているアイテム
    /// </summary>
    public IActionable HoldObject
    {
        get => _holdItem;
        set
        {
            if (value == null || value != _holdItem)
            {
                _holdItem = value;
            }
        }
    }
}
