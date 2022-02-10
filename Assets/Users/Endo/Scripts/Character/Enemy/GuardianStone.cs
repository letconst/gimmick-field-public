using Cysharp.Threading.Tasks;
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

        // SE再生
        SoundManager.PlaySound(SoundDef.StoneCollision_SE, position: transform.position);

        // 破棄処理
        SpawnParticle();
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

    private async void SpawnParticle()
    {
        ParticlePlayer particle = SceneControllerBase.Instance.dustParticlePool.Rent();
        particle.transform.position = transform.position;
        particle.PlayParticle();

        // パーティクルが停止するまで待機
        while (!particle.selfParticle.isStopped)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        MainGameController.Instance.dustParticlePool.Return(particle);
    }
}
