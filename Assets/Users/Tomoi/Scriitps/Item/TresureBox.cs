using UnityEngine;

public class TresureBox : MonoBehaviour,IActionable
{
    private Animator _animator;
    private bool     isOpen = false;

    private static readonly int Open = Animator.StringToHash("Open");

    public HandType RequireHand { get; private set; }

    void Start()
    {
        _animator   = GetComponent<Animator>();
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
            _animator.SetTrigger(Open);
            Gamemaneger.Instance.OnClear();
        }
        isOpen = true;
    }

    public void DeAction()
    {

    }
}
