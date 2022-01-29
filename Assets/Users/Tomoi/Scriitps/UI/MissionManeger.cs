using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

[Serializable]
public class Mission
{
    public MissionManeger.MissionType MissionType;
    public string MisstionText;
}
public class MissionManeger : SingletonMonoBehaviour<MissionManeger>
{
    [SerializeField] private Animator _StampAnimator;
    [SerializeField] private Text TextArea;
    public enum MissionType
    {
        OpendTresureBox,
        OpendDoor
    }

    [SerializeField,Header("ミッションの項目 上から順番に処理されます")] private List<Mission> MissitonList = new List<Mission>();

    private Subject<MissionType> _CompleteMission = new Subject<MissionType>();
    public IObserver<MissionType> CompleteMissionObserver => _CompleteMission;
    private IObservable<MissionType> CompleteMissionObservable => _CompleteMission;

    /*
     * 別のスクリプトで
     * MissionManeger.Instance.CompleteMissionObserver.OnNext(MissionManeger.MissionType.OpendTresureBox);
     * のように書くことでミッションが達成したときの処理が実行可能
     */


    void Start()
    {
        CheckMission();
        CompleteMissionObservable
            .Where(x =>
                x == MissitonList[0].MissionType
            )
            .Subscribe(_ =>
            {
                _StampAnimator.SetTrigger("isStamp");
            })
            .AddTo(this);

        if (_StampAnimator)
        {
            ObservableStateMachineTrigger trigger = _StampAnimator.GetBehaviour<ObservableStateMachineTrigger>();
            trigger
                .OnStateExitAsObservable()
                .Where(x => x.StateInfo.IsName("Stamp")) // すべてのStateを対象にする場合は不要
                .Subscribe(x =>
                {
                    _StampAnimator.ResetTrigger("isStamp");
                    MissitonList.RemoveAt(0);
                    CheckMission();
                })
                .AddTo(this);
        }
    }

    void Update()
    {

    }

    void CheckMission()
    {
        if (MissitonList.Count != 0)
        {
            TextArea.text = MissitonList[0].MisstionText;
        }
        else
        {
            TextArea.text = "";
        }
    }
}
