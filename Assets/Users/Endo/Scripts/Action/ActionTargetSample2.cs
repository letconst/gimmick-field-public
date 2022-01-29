using UnityEngine;

public class ActionTargetSample2 : MonoBehaviour, IActionable
{
    public void Action()
    {
        Debug.Log("calling test from ATS2");
    }

    public void DeAction()
    {
    }
    public bool _isOutline { get; private set; }
    public bool isGrab { get;private set; }

    public HandType RequireHand { get; }

    public void ShowOutline()
    {
        gameObject.layer = 9;
    }

    public void HideOutline()
    {
        gameObject.layer = 0;
    }
}
