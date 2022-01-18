using UnityEngine;

public class ActionTargetSample1 : MonoBehaviour, IActionable
{
    public void Action()
    {
        Debug.Log("calling test from ATS1");
    }

    public void DeAction()
    {
    }
    public bool _isOutline { get; private set; }
    public HandType RequireHand { get; }
    public void ShowOutline(){}
    public void HideOutline(){}

}
