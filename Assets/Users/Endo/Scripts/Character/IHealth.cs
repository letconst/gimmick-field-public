public interface IHealth
{
    /// <summary>最大体力値</summary>
    public int MaxHealth { get; }

    /// <summary>現在の体力値</summary>
    public int CurrentHealth { get; }

    /// <summary>死亡しているか</summary>
    public bool IsDead { get; }

    /// <summary>
    /// ダメージを受けた際の処理
    /// </summary>
    /// <param name="damageAmount">受けたダメージ量</param>
    /// <param name="attackedObject">攻撃をしてきたオブジェクト</param>
    public void OnDamaged(int damageAmount, UnityEngine.GameObject attackedObject);
}
