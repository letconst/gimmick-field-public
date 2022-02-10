using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class CreateStarStonePillar : MonoBehaviour, IActionable
{
    [SerializeField,Header("5以上の任意の値")] private int MaxHitCount;
    private int HitCount = 0;
    // 5回くらい沈むとコンプリート

    private const int SinkCount = 5;

    /// <summary>HitCountと比較する値 </summary>
    private float SinkJudgmentValue;

    private bool isHit = false;
    private bool isHited = false;

    private int ratio = 1;

    [SerializeField, Header("星を制する場所をセット")]
    private List<GameObject> StarList;
    [SerializeField,Header("一回叩くごとに沈む量")] private float AmountSink;
    [SerializeField] private GameObject StarPool;

    private CreateStarStonePillarChecker _instance;

    private IObservable<Unit> _subject;

    public bool _isOutline { get; private set; }
    public HandType RequireHand { get; private set; }
    public bool isGrab { get; private set; }

    private void Start()
    {
        _instance = CreateStarStonePillarChecker.Instance;

        _subject = CreateStarStonePillarChecker.Instance.OnColliderEnterHand;

        _subject.Where(_=>isHit && !isHited).Subscribe(_=>AddCount()).AddTo(this);

        MaxHitCount += 2;
        SinkJudgmentValue = (float)MaxHitCount / (float)SinkCount;

        isGrab      = true;
        _isOutline  = true;
        RequireHand = HandType.One;

        foreach (GameObject star in StarList)
        {
            star.SetActive(false);
        }
    }

    private void AddCount()
    {
        // 釘を打つように段々に沈んでいく
        HitCount++;
        Debug.Log("HitCount" + HitCount);

        // 5回くらい沈むとコンプリート
        int i = (int)(HitCount / SinkJudgmentValue);

        if (HitCount >= SinkJudgmentValue * ratio)
        {
            ratio++;
            StarPool.transform.position = new Vector3(StarPool.transform.position.x,StarPool.transform.position.y - AmountSink,StarPool.transform.position.z);
        }

        if (HitCount >= MaxHitCount)
        {
            isHited = true;
            _isOutline  = false;

            //星を表示
            foreach (GameObject gameObject in StarList)
            {
                gameObject.SetActive(true);
            }
        }
    }

    public void Action(HandType handType)
    {
        isHit = true;
        PlayerHandController.SetSpiderwebCheckHandActive(true,handType,false);
    }

    public void DeAction(HandType handType)
    {
        isHit = false;
        PlayerHandController.SetSpiderwebCheckHandActive(false,handType);
    }

    public void ShowOutline()
    {
        StarPool.layer = 9;
    }

    public void HideOutline()
    {
        StarPool.layer = 0;
    }
}
