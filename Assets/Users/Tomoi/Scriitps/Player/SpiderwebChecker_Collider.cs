using System;
using UnityEngine;


public class SpiderwebChecker_Collider : MonoBehaviour
{
    [Header("右手か左手か"),SerializeField]
    private SpiderwebChecker.Hand _hand ;

    [Header("手を中心にしてColliderのある場所"),SerializeField]
    private SpiderwebChecker.ColliderPosition _colliderPosition ;


    [Header("判定する手のオブジェクト"),SerializeField]
    private GameObject handGameObject;

    [Header("親オブジェクトのSpiderwebChecker"),SerializeField]
    private SpiderwebChecker _spiderwebChecker;

    
    private void OnTriggerEnter(Collider obj)
    {
        if (obj.gameObject == handGameObject)
        {
            _spiderwebChecker.CheckTilt(_hand, _colliderPosition);
        }
    }
}
