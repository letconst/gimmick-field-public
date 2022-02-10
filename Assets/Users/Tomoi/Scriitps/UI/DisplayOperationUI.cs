using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayOperationUI : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private UIAnimationList uiAnimationList;
    public enum UIAnimationList
    {
        Doar,
        SpiderWeb,
        TresureBox,
        Stone,
        Rock,
        Star,
        CreateStarStonePillar
    }
    [SerializeField] private float displayTime;
     private float time;

     GameObject _gameObject;
     bool isDisplayed = false;

     private void Update()
     {
         if (isDisplayed)
         {
             time += Time.deltaTime;
         }
     }

     private bool ChekePlayer(Collider collider)
    {
        if (collider.gameObject == null) return false;
        
        if (!isDisplayed && collider.gameObject.tag == "Player") return true;
        
        return false;
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (!isDisplayed && ChekePlayer(collider))
        {
            _gameObject = collider.gameObject;

            time = 0;
            isDisplayed = true;
            animator.SetBool(uiAnimationList.ToString(), true);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        //修正版
        if (_gameObject == collider?.gameObject)
        {
            animator.SetBool(uiAnimationList.ToString(), false);

            if (time < displayTime) isDisplayed = false;
        }
        //なんかうまく動かなかった
        /*if (isDisplayed && ChekePlayer(collider))
        {

            animator.SetBool(animatorBoolName, false);

            if (time < displayTime) isDisplayed = false;
        }*/
    }
}
