using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractive : MonoBehaviour
{
    private bool isAnimating;

    private WaitForSeconds pause = new WaitForSeconds(0.04f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAnimating)
        {
            // if other is on the left, shake to the right
            if(other.transform.position.x < transform.position.x)
            {
                StartCoroutine(RotateRight());
            }
            else // otherwise, shake to the left
            {
                StartCoroutine(RotateLeft());
            }

            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isAnimating)
        {
            // if other is on the left, shake to the right
            if (other.transform.position.x > transform.position.x)
            {
                StartCoroutine(RotateRight());
            }
            else // otherwise, shake to the left
            {
                StartCoroutine(RotateLeft());
            }
        }
    }

    private IEnumerator RotateLeft()
    {
        isAnimating = true;

        for(int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0, 0, 2);
            yield return pause;
        }

        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0, 0, -2);
            yield return pause;
        }

        transform.GetChild(0).Rotate(0, 0, 2);
        yield return pause;

        isAnimating = false;
    }

    private IEnumerator RotateRight()
    {
        isAnimating = true;

        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0, 0, -2);
            yield return pause;
        }

        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0, 0, 2);
            yield return pause;
        }

        transform.GetChild(0).Rotate(0, 0, -2);
        yield return pause;

        isAnimating = false;
    }
}
