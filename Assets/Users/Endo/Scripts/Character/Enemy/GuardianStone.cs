using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GuardianStone : MonoBehaviour
{
    [SerializeField]
    private int damageAmount;

    private Rigidbody _selfRig;

    private void OnEnable()
    {
        if (!_selfRig)
        {
            _selfRig = GetComponent<Rigidbody>();
        }
    }

    private void OnDisable()
    {
        // 慣性をリセット
        _selfRig.velocity        = Vector3.zero;
        _selfRig.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var h = collision.collider.GetComponent<IHealth>();

        // プレイヤーならダメージを与える
        if (collision.collider.CompareTag("Player"))
        {
            h?.OnDamaged(damageAmount, gameObject);
        }

        // 破棄処理
        // TODO: エフェクト
        Guardian.Instance.stonePool.Return(this);
    }

    /// <summary>
    /// 自身に力を加える
    /// </summary>
    /// <param name="force">力量</param>
    public void AddForce(Vector3 force)
    {
        _selfRig.AddForce(force, ForceMode.VelocityChange);
    }
}
