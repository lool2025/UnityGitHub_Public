using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }
    //Öð½¥»Ø¸´ÑÕÉ«
    public void FadeIn()
    {
        Color targetColor = new Color(1, 1, 1, 1);
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }

    //Öð½¥ÐÞ¸ÄÑÕÉ«
    public void FadeOut()
    {
        Color targetColor = new Color(1, 1, 1, Settings.targetAloha);
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }
}
