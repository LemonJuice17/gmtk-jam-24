using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackFade : MonoBehaviour
{
    public float FadeTime = 1;

    public void FadeIn() => GameManager.instance?.BlackScreenReference?.CrossFadeAlpha(1, FadeTime, false);
    public void FadeOut() => GameManager.instance?.BlackScreenReference?.CrossFadeAlpha(0, FadeTime, false);
}
