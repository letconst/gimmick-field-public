using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WeakWall : MonoBehaviour
{
    [SerializeField, Header("破壊時の砂煙パーティクル")]
    private ParticlePlayer dustParticle;

    [SerializeField, Header("破壊時に破片を物理挙動させるか")]
    private bool isBreakAsShard;

    [SerializeField, Header("破壊後、壁が破棄されるまでの秒数"), Range(0, 3)]
    private float destroyingSeconds;

    private ParticlePool _dustParticlePool;

    private List<WeakWallShard> _shards;

    private bool _isBreaked;

    private void Start()
    {
        _dustParticlePool = new ParticlePool(dustParticle);

        if (isBreakAsShard)
        {
            _shards = new List<WeakWallShard>(GetComponentsInChildren<WeakWallShard>());
        }
    }

    /// <summary>
    /// 壁を崩す
    /// </summary>
    private void BreakWall()
    {
        SpawnParticle(() => Destroy(gameObject), destroyingSeconds);
    }

    /// <summary>
    /// 煙パーティクルを出現させる
    /// </summary>
    /// <param name="callback">出現中に実行するコールバック</param>
    /// <param name="callbackInvokeRate">パーティクルがどのくらいの割合再生されたらコールバックが実行されるか (0-1)</param>
    private async void SpawnParticle(System.Action callback, float callbackInvokeRate)
    {
        ParticlePlayer particle1 = _dustParticlePool.Rent();
        ParticlePlayer particle2 = _dustParticlePool.Rent();
        particle1.transform.position = transform.position;
        particle2.transform.position = transform.position;
        particle1.transform.rotation = transform.rotation * Quaternion.Euler(0, 90, 90);
        particle2.transform.rotation = transform.rotation * Quaternion.Euler(0, 90, -90);

        particle1.PlayParticle();
        particle2.PlayParticle();

        float duration          = particle1.selfParticle.main.duration;
        bool  isCallbackInvoked = false;

        // パーティクルが停止するまで待機
        while (!particle1.selfParticle.isStopped)
        {
            // コールバックがあれば、指定割合で実行
            if (callback != null)
            {
                float playRate = particle1.selfParticle.time / duration;

                if (!isCallbackInvoked && callbackInvokeRate <= playRate)
                {
                    isCallbackInvoked = true;
                    callback();
                }
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        _dustParticlePool.Return(particle1);
        _dustParticlePool.Return(particle2);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤーまたは守護者の投げた石があたった場合に破壊処理
        if (collision.collider.CompareTag("PlayerStone") || collision.collider.CompareTag("GuardianStone"))
        {
            // 重複防止
            if (_isBreaked) return;

            _isBreaked = true;

            BreakWall();

            // 破片を物理動かすなら各破片に重力を設定
            if (isBreakAsShard)
            {
                foreach (WeakWallShard shard in _shards)
                {
                    shard.EnableGravity();
                }
            }
        }
    }
}
