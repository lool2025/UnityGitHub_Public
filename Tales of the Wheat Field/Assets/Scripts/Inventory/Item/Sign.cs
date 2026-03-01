using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{

    public  bool canOpen=false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[m]");
            canOpen = true;
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canOpen = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
