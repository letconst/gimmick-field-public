using UnityEngine;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    [SerializeField]
    private Transform playerHandTrf;

    public Transform PlayerHandTrf => playerHandTrf;

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
