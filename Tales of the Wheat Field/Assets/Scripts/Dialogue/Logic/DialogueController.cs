using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace MFarm.Dialogue 
{
    //[RequireComponent(typeof(NPCManager))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        public NPCMovement npc=>GetComponent<NPCMovement>();

        public UnityEvent OnFinishEvent;

        public List<DialoguePiece> dialogueList=new List<DialoguePiece>();
        public Stack<DialoguePiece> dialogueStack;
        private bool canTalk;
        private GameObject uiSign;
        private bool isTalking;

        private void Awake()
        {
            uiSign = transform.GetChild(1).gameObject;
            FillDialogueStack();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canTalk = !npc.isMoving && npc.interactable;
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canTalk = false;
            }
        }

        public void Update()
        {
            uiSign.SetActive(canTalk);
            if(canTalk&&Input.GetKeyDown(KeyCode.Space)&&!isTalking)
            {
                StartCoroutine(DailogueRoutine());
            }
        }
        /// <summary>
        /// 构建对话堆栈
        /// </summary>
        private void FillDialogueStack()
        {
            dialogueStack=new Stack<DialoguePiece>();
            for(int i=dialogueList.Count-1; i > -1; i--)
            {
                dialogueList[i].isDone = false;
                dialogueStack.Push(dialogueList[i]);
            }
        }

        private IEnumerator DailogueRoutine()
        {
            isTalking = true;
            if(dialogueStack.TryPop(out DialoguePiece result))
            {
                //传到UI显示对话
                EventHandler.CallShowDialogueEvent(result);
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                yield return new WaitUntil(()=>result.isDone);
                isTalking=false;
            }
            else
            {
               
                EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
                EventHandler.CallShowDialogueEvent(null);
                FillDialogueStack();
                isTalking=false;
                
                if(OnFinishEvent != null)
                {
                    OnFinishEvent?.Invoke();
                    canTalk = false;
                }

            }
        }

    }

}

