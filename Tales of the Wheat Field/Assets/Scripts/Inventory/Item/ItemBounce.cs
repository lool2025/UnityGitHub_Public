using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
       private Transform spritemTrans;

        private BoxCollider2D coll;

        public float gravity = -3.5f;

        private bool isGround;

        private float distance;

        private Vector2 direction;
        private Vector3 targetPos;


        public void Awake()
        {
            spritemTrans = transform.GetChild(0);
            coll=GetComponent<BoxCollider2D>();
            coll.enabled=false;
        }

        public void Update()
        {
            Bounce();
        }

        public void InitBounceItem(Vector3 target,Vector2 dir)
        {
            coll.enabled=false;
            direction = dir;
            targetPos=target;
            distance=Vector3.Distance(target,transform.position);

            spritemTrans.position += Vector3.up * 1.5f;

        }

        private void Bounce()
        {
            isGround=spritemTrans.position.y<=transform.position.y;
            if(Vector3.Distance(transform.position,targetPos) > 0.1f)
            {
                transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;

            }
            if(!isGround)
            {
                print("123");
                spritemTrans.position += Vector3.up * gravity * Time.deltaTime;
            }
            else
            {
                spritemTrans.position=transform.position;
                coll.enabled=true;
            }
        }

    }

}
