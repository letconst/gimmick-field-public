using System;
using UniRx;

public class SpiderwebChecker : SingletonMonoBehaviour<SpiderwebChecker>
{
    private bool _rightHand = false;
    private bool _leftHand  = false;

    public enum Hand{
        right,
        left
    }
    public enum ColliderPosition{
        Inside,
        Outside
    }

    private Subject<ColliderPosition>     _OnColliderEnterHand = new Subject<ColliderPosition>();
    public  IObservable<ColliderPosition> OnColliderEnterHand => _OnColliderEnterHand;

    public void CheckTilt(Hand _hand, ColliderPosition _colliderPosition)
    {
        //右手
        if (_hand == Hand.right)
        {
            //false != true または true != falseなら _rightHandの状態を反転する
            if (_rightHand != isColliderPosition(_colliderPosition))
            {
                _rightHand = !_rightHand;
                _OnColliderEnterHand.OnNext(_colliderPosition);
            }
        }else if (_hand == Hand.left)
        {
            //false != true または true != falseなら _leftHandの状態を反転する
            if (_leftHand != isColliderPosition(_colliderPosition))
            {
                _leftHand = !_leftHand;
                _OnColliderEnterHand.OnNext(_colliderPosition);
            }
        }
    }

    //外側ならtrue内側ならfalseを返す
    bool isColliderPosition(ColliderPosition _colliderPosition)
    {
        if (_colliderPosition == ColliderPosition.Inside)
        {
            return true;
        }
        return false;
    }
}
