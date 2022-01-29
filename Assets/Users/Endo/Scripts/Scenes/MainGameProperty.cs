using UnityEngine;

public class MainGameProperty : SingletonMonoBehaviour<MainGameProperty>
{
    [SerializeField, Header("煙パーティクルの親オブジェクト")]
    public Transform dustParticleParent;
}
