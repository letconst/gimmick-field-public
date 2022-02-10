using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlePlayer : MonoBehaviour
{
    [HideInInspector]
    public ParticleSystem selfParticle;

    private void OnEnable()
    {
        if (!selfParticle)
        {
            selfParticle = GetComponent<ParticleSystem>();
        }
    }

    /// <summary>
    /// パーティクルを再生する
    /// </summary>
    public void PlayParticle()
    {
        selfParticle.Play();
    }
}
