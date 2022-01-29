using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionTest : MonoBehaviour
{
    [Header("一度Trueにしたら再度実行不可能")]
    [SerializeField]private bool isOpendTresureBox = false;
    private bool isOpendTresureBoxTrued = false;
    [SerializeField]private bool isOpendDoor = false;
    private bool isOpendDoorTrued = false;
    void Start()
    {
        
    }

    void Update()
    {
        if (isOpendTresureBox && !isOpendTresureBoxTrued)
        {
            MissionManeger.Instance.CompleteMissionObserver.OnNext(MissionManeger.MissionType.OpendTresureBox); 
            isOpendTresureBoxTrued = isOpendTresureBox;
        }
        if (isOpendDoor && !isOpendDoorTrued)
        {
            MissionManeger.Instance.CompleteMissionObserver.OnNext(MissionManeger.MissionType.OpendDoor); 
            isOpendDoorTrued = isOpendDoor;
        }
    }
}
