using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class Guardian : EnemyBase
{
    [SerializeField, Header("この守護者を倒すことで開けるようになる扉")]
    private LockDoar lockDoar;

    [SerializeField, Header("攻撃時に生成する石オブジェクト")]
    private GameObject[] stonePrefabs;

    [SerializeField, Header("石が生成される中心からの位置")]
    private float stoneGeneratedDistance;

    [SerializeField, Header("攻撃時の石の速度")]
    private float stoneAttackSpeed;

    [SerializeField, Min(1), Header("攻撃に遷移するまでの最小時間")]
    private int minAttackInterval;

    [SerializeField, Min(2), Header("攻撃に遷移するまでの最大時間")]
    private int maxAttackInterval;

    private static Guardian _instance;

    private Player _player;

    private GuardianState _selfState;

    public GuardianStonePool stonePool;

    public static Guardian Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<Guardian>();

            return _instance;
        }
    }

    public enum GuardianState
    {
        Idle,
        Stun,
        Attacking
    }

    private void Awake()
    {
        CheckInstance();

        stonePool = new GuardianStonePool(stonePrefabs);
    }

    protected override async void Start()
    {
        base.Start();

        _player    = Player.Instance;
        _selfState = GuardianState.Idle;

        this.ObserveEveryValueChanged(x => x._selfState).Subscribe(OnStateChanged).AddTo(this);
    }

    private void Update()
    {
        if (_selfState == GuardianState.Idle)
        {
            LookAtPlayer();
        }
    }

    /// <summary>
    /// Guardianインスタンスがすでにあるか確認し、ある場合は自身を破棄する
    /// </summary>
    private void CheckInstance()
    {
        if (!_instance)
        {
            _instance = this;

            return;
        }

        if (_instance != this)
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// State変更時の処理
    /// </summary>
    /// <param name="state">変更後のState</param>
    private async void OnStateChanged(GuardianState state)
    {
        switch (state)
        {
            case GuardianState.Idle:
            {
                // Idleから数秒後に攻撃（仮）
                await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(minAttackInterval, maxAttackInterval)));

                _selfState = GuardianState.Attacking;

                break;
            }

            case GuardianState.Stun:
                break;

            case GuardianState.Attacking:
            {
                Attack1();

                _selfState = GuardianState.Idle;

                break;
            }
        }
    }

    /// <summary>
    /// プレイヤーの方向へ向かせる
    /// </summary>
    private void LookAtPlayer()
    {
        Vector3 norm = _player.transform.position - transform.position;
        norm.y             = 0;
        transform.rotation = Quaternion.LookRotation(norm);
    }

    /// <summary>
    /// 攻撃パターン1。向いてる方向を軸として、十字に石を飛ばす
    /// </summary>
    private void Attack1()
    {
        Vector3 selfPos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 right   = transform.right;

        // 各方向への生成位置
        Vector3 y         = transform.up * 2;
        Vector3 stonePosF = selfPos                                    + forward * stoneGeneratedDistance + y;
        Vector3 stonePosL = selfPos - right * stoneGeneratedDistance   + y;
        Vector3 stonePosR = selfPos                                    + right * stoneGeneratedDistance + y;
        Vector3 stonePosB = selfPos - forward * stoneGeneratedDistance + y;

        // 各石を生成
        GuardianStone stoneF = stonePool.Rent();
        GuardianStone stoneL = stonePool.Rent();
        GuardianStone stoneR = stonePool.Rent();
        GuardianStone stoneB = stonePool.Rent();

        // 座標設定
        stoneF.transform.position = stonePosF;
        stoneL.transform.position = stonePosL;
        stoneR.transform.position = stonePosR;
        stoneB.transform.position = stonePosB;

        // 発射
        stoneF.AddForce(forward  * stoneAttackSpeed);
        stoneL.AddForce(-right   * stoneAttackSpeed);
        stoneR.AddForce(right    * stoneAttackSpeed);
        stoneB.AddForce(-forward * stoneAttackSpeed);
    }

    public override void OnDamaged(int damageAmount, GameObject attackedObject)
    {
        DecreaseHealth(damageAmount);

        // 死亡時
        if (IsDead)
        {
            // トリガーとする扉があればロック解除
            if (lockDoar)
            {
                lockDoar.unLock(true);
            }

            // TODO: エフェクトなど
            Destroy(gameObject);
        }
    }
}
