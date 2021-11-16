using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TresureBox : MonoBehaviour,IActionable
{
    private Animation _animation;
    private bool isOpen = false;

    public HandType RequireHand { get; private set; }

    void Start()
    {
        _animation = gameObject.GetComponentInChildren<Animation>();

        RequireHand = HandType.Both;
    }

    void Update()
    {

        if (isOpen)
        {

        }

    }

    public void Action()
    {
        if(!isOpen)
        {
            _animation.Play();
        }
        isOpen = true;
    }

    public void DeAction()
    {

    }
}
