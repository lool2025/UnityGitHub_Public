using System.Collections;
using System.Collections.Generic;
using MFarm.Dialogue;
using UnityEngine;

namespace MFarm.Transition
{
    public class Teleport : MonoBehaviour
    {
        public string sceneToGo;

        public Vector3 positionToGo;


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
               
                EventHandler.CallTransitionEvent(sceneToGo,positionToGo);
            }
        }
    }
}

