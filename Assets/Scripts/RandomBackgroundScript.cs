using UnityEngine;
using System.Collections;

public class RandomBackgroundScript : MonoBehaviour
{
    public Sprite[] Backgrounds;

    private SpriteRenderer spriteRenderer;
    private int lastIndex = -1;

    void Start()
    {
        spriteRenderer = GetComponent<Renderer>() as SpriteRenderer;
        SetRandomBackground();
    }

    void SetRandomBackground()
    {
        if (Backgrounds.Length == 0) return;

        int newIndex;
        do
        {
            newIndex = Random.Range(0, Backgrounds.Length);
        } while (newIndex == lastIndex && Backgrounds.Length > 1); // avoid repeating last

        lastIndex = newIndex;
        spriteRenderer.sprite = Backgrounds[newIndex];
    }
}
