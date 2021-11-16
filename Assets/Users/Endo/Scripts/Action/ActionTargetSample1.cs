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

    public HandType RequireHand { get; }
}
