using DG.Tweening;
using UnityEngine;

// the Object must have the spriteRenderer
[RequireComponent(typeof(SpriteRenderer))]

public class ItemFader : MonoBehaviour
{
    // get this element
    private SpriteRenderer spriteRenderer;

    // call at the start to assign the value to this variable
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // to call this method after the situation was finished
    /// <summary>
    /// Back to normal colour gradually
    /// </summary>
    public void FadeIn()
    {
        // we set the color of the target
        Color targetColor = new Color(1, 1, 1, 1);

        // call the DOTween method to change the color with specific time
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);

    }

    // to call this method after the situation was triggered
    /// <summary>
    /// Change to the translucent color
    /// </summary>
    public void FadeOut()
    {
        // we set the color of the target
        Color targetColor = new Color(1, 1, 1, Settings.targetAlpha);

        // call the DOTween method to change the color with specific time
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }

}
