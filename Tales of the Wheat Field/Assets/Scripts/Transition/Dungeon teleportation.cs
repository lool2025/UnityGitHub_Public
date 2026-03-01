using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeonteleportation : MonoBehaviour
{
    public ConfirmPiece ConfirmPiece;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EventHandler.CallShowConfirmEvent(ConfirmPiece);
        }
    }
}
