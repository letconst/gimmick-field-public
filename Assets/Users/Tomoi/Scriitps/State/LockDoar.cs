using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDoar : MonoBehaviour,LockInterface
{
    public bool isLock { get; set; }
    [SerializeField,Header("星をはめる台座の数")] private int _pedestalsFitStarsCount = 0;

    public void unLock(bool _unLockCountSkip = false)
    {
        if (!_unLockCountSkip)
        {
            _pedestalsFitStarsCount--;
            if (0 >= _pedestalsFitStarsCount)
            {
                Debug.Log("unlocked");
                //ロックの解除
                isLock = false;
            }
        }else {
            //ロックの解除
            isLock = false;
        }
    }

    void Start()
    {
        isLock = true;
    }
}