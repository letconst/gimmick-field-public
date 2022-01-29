using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : MonoBehaviour
{
    private GameObject Star = null;
    private Vector3 SetStarPosition;
    [SerializeField] private LockDoar _unLockObject;
    [SerializeField, Range(0, 5)] private float _SetYPosition;
    [SerializeField, Range(-1, 1)] private float _SetZPosition;

    [SerializeField] private bool _isDebug = false;
    private bool _isInset = false;

    Star _starScript;
    private void Start()
    {
        SetStarPosition = this.gameObject.transform.position;
        SetStarPosition.y += _SetYPosition;
        SetStarPosition.z += _SetZPosition;

        if (_isDebug)
        {
            _unLockObject.unLock();
        }
    }

    private void Update()
    {
        if (Star != null)
        {
            Star.transform.position = SetStarPosition;
        }
    }

    private void OnTriggerEnter(Collider hit)
    {
        if (!_isInset)
        {
            _starScript = hit.gameObject.GetComponent<Star>() ?? null;
             
            if (_starScript != null)
            {
                //Starを持てなくする処理
                _starScript.SetStarOnPosition();
                //Starを保持
                Star = hit.gameObject;
                //ここに扉のロックを解除する処理
                _unLockObject.unLock();

                _isInset = true;
            }
        }
    }
}
