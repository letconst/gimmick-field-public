using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

public class CreateStarStonePillarChecker : SingletonMonoBehaviour<CreateStarStonePillarChecker>
{
    [SerializeField] private GameObject LeftHand, RightHand;
    
    private Subject<Unit>     _OnColliderEnterHand = new Subject<Unit>();
    public  IObservable<Unit> OnColliderEnterHand => _OnColliderEnterHand;
    void Start()
    {
        
    }
    

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject == LeftHand || hit.gameObject == RightHand)
        {
            _OnColliderEnterHand.OnNext(Unit.Default);
        }
    }
}
