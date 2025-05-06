using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItemFader : MonoBehaviour
{

    // if the collision happens, call this method
    private void OnTriggerEnter2D(Collider2D other)
    {
        // this method is to get the children components of the object having the script because trunk and top are both childern of tree
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();

        // if this collision object has faders
        if(faders.Length > 0)
        {
            foreach(var item in faders)
            {
                item.FadeOut();
            }
        }

    }

    // if the collision exits, call this method
    private void OnTriggerExit2D(Collider2D other)
    {
        // this method is to get the children components of the object having the script because trunk and top are both childern of tree
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();

        // if this collision object has faders
        if (faders.Length > 0)
        {
            foreach (var item in faders)
            {
                item.FadeIn();
            }
        }
    }
}
