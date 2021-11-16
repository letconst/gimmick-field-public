using UnityEngine;
using UnityEngine.UI;

public class SystemProperty : SingletonMonoBehaviour<SystemProperty>
{
    [SerializeField]
    private CanvasGroup fadeCanvasGroup;

    public static CanvasGroup FadeCanvasGroup => Instance.fadeCanvasGroup;

    [SerializeField]
    private Image fadeImage;

    public static Image FadeImage => Instance.fadeImage;
}
