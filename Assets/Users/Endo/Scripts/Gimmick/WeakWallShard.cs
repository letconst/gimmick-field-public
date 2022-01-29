using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WeakWallShard : MonoBehaviour
{
    private Rigidbody _selfRig;

    private void Start()
    {
        _selfRig             = GetComponent<Rigidbody>();
        _selfRig.useGravity  = false;
        _selfRig.isKinematic = true;
    }

    public void EnableGravity()
    {
        _selfRig.useGravity  = true;
        _selfRig.isKinematic = false;
    }
}
