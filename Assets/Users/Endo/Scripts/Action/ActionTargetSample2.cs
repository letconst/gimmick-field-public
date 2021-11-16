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

    public HandType RequireHand { get; }
}
