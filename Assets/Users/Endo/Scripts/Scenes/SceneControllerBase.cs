using UnityEngine;

public abstract class SceneControllerBase : SingletonMonoBehaviour<SceneControllerBase>
{
    [SerializeField]
    private ParticlePlayer dustParticle;

    public ParticlePool dustParticlePool { get; private set; }

    protected virtual void Start()
    {
        dustParticlePool = new ParticlePool(dustParticle);
    }
}
