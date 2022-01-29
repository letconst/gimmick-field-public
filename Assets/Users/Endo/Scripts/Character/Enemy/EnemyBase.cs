using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IHealth
{
    [SerializeField]
    private int maxHealth;

    public float Health { get; private set; }

    public int  MaxHealth     { get; private set; }
    public int  CurrentHealth { get; private set; }
    public bool IsDead        { get; private set; }

    public abstract void OnDamaged(int damageAmount, GameObject attackedObject);

    protected virtual void Start()
    {
        MaxHealth     = maxHealth;
        CurrentHealth = maxHealth;
    }

    /// <summary>
    /// 体力を減少させる
    /// </summary>
    /// <param name="decrease">減少量</param>
    protected void DecreaseHealth(int decrease)
    {
        CurrentHealth -= decrease;

        if (CurrentHealth <= 0)
        {
            IsDead = true;
        }
    }
}
