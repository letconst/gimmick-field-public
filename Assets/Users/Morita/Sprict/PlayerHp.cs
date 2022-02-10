using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHp : MonoBehaviour, IHealth
{
    [SerializeField, Range(0, 3), Tooltip("HPの初期値")]
    private int Health;

    [SerializeField]
    private List<Image> HPUI;

    [SerializeField, Header("ダメージを受けた際、ダメージエフェクトが表示され続ける秒数"), Range(.1f, 3)]
    private float damageEffectShowSeconds;

    [SerializeField, Header("ダメージエフェクトが消える際のフェード秒数"), Range(.1f, 5)]
    private float damageEffectFadeSeconds;

    [SerializeField, Header("ライフのダメージエフェクト用画像 (初期状態画像は除く)")]
    private Sprite[] lifeDamageEffectSprites;

    [SerializeField, Header("ライフのダメージエフェクトの再生秒数")]
    private float lifeDamageEffectSeconds;

    /// <summary>最大体力値</summary>
    public int MaxHealth { get; private set; }

    /// <summary>現在の体力値</summary>
    public int CurrentHealth { get; private set; }

    /// <summary>死亡しているか</summary>
    public bool IsDead { get; private set; }

    /// <summary>
    /// ダメージを受けた際の処理
    /// </summary>
    /// <param name="damageAmount">受けたダメージ量</param>
    /// <param name="attackedObject">攻撃をしてきたオブジェクト</param>
    public void OnDamaged(int damageAmount, UnityEngine.GameObject attackedObject)
    {
        // 死んでいるなら処理しない
        if (IsDead) return;

        CurrentHealth -= damageAmount;
        UpdateHPUI(CurrentHealth + damageAmount);
        PostEffectController.Instance.PlayDamageEffect(damageEffectShowSeconds, damageEffectFadeSeconds);

        SoundManager.PlaySound(SoundDef.Damaged_SE);

        // 体力0ならゲームオーバー処理
        if (CurrentHealth <= 0)
        {
            IsDead = true;
            Gamemaneger.Instance.SetGameStateToResult(false);
        }
    }

    private void Awake()
    {
        //最大HPの初期化
        MaxHealth = Health;
    }

    private void Start()
    {
        //現在HPを初期化
        CurrentHealth = MaxHealth;
    }

    /// <summary>
    /// UIを更新する関数
    /// </summary>
    /// <param name="beforeHp">ダメージを受ける前の体力値</param>
    private void UpdateHPUI(int beforeHp)
    {
        for (int i = beforeHp; i > CurrentHealth; i--)
        {
            PlayLifeDamageEffect(i - 1);
        }
    }

    /// <summary>
    /// ライフのダメージエフェクトを再生する
    /// </summary>
    /// <param name="i">何番目のライフ画像か</param>
    private async void PlayLifeDamageEffect(int i)
    {
        // ライフ画像が徐々に切り替わるタスク
        var updateLifeEffectTask = UniTask.Create(async () =>
        {
            float timeElapsed = 0; // 再生経過時間
            float playRatio   = 0; // 再生経過時間の比 (0-1)

            while (playRatio < 1)
            {
                int currentDamageIndex = Mathf.FloorToInt(lifeDamageEffectSprites.Length * playRatio);
                HPUI[i].sprite = lifeDamageEffectSprites[currentDamageIndex];

                await UniTask.Yield(PlayerLoopTiming.Update);

                timeElapsed += Time.deltaTime;
                playRatio   =  timeElapsed / lifeDamageEffectSeconds;
            }
        });

        // ライフ画像が徐々に透明になっていくタスク
        UniTask spriteFadeTask = FadeTransition.FadeOut(HPUI[i], lifeDamageEffectSeconds);

        await UniTask.WhenAll(updateLifeEffectTask, spriteFadeTask);
    }
}
