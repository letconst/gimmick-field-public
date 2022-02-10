using UniRx.Toolkit;
using UnityEngine;

public class ParticlePool : ObjectPool<ParticlePlayer>
{
    private readonly ParticlePlayer _prefab;

    public ParticlePool(ParticlePlayer prefab)
    {
        _prefab = prefab;
    }

    protected override ParticlePlayer CreateInstance()
    {
        Transform parent = null;

        if (MainGameProperty.Instance)
        {
            parent = MainGameProperty.Instance.dustParticleParent;
        }

        return Object.Instantiate(_prefab, parent);
    }
}
