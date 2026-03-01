using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MFarm.Dialogue;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmUI : MonoBehaviour
{
    public GameObject dialogueBox;
    public Text dialogueText;
    public Image  faceLeft;
    public Text  nameLeft;

    [Header("目标场景")]public string sceneToGo;
    [Header("目标地点")] public Vector3 positionToGo;

    private void Awake()
    {
        dialogueBox.SetActive(false);
    }
    private void OnEnable()
    {
        EventHandler.ShowConfirmEvent += OnShowConfirmEvent;
    }
    private void OnDisable()
    {
        EventHandler.ShowConfirmEvent -= OnShowConfirmEvent;
    }

    private void OnShowConfirmEvent(ConfirmPiece piece)
    {

        StartCoroutine(ShowDialogue(piece));
    }
    private IEnumerator ShowDialogue(ConfirmPiece piece)
    {
        if (piece.isTransmitting) 
        {
            sceneToGo= piece.sceneToGo;
            positionToGo= piece.positionToGo;
        }

        if (piece != null)
        {

            dialogueBox.SetActive(true);
            dialogueText.text = string.Empty;
            if (piece.name != string.Empty)
            {
                    faceLeft.gameObject.SetActive(true);
                    faceLeft.sprite = piece.faceImage;
                    nameLeft.text = piece.name;
            }
            else
            {
                faceLeft.gameObject.SetActive(false);  
                nameLeft.gameObject.SetActive(false);
              
            }
            yield return dialogueText.DOText(piece.dialogueText, 1f).WaitForCompletion();
   
            
        }
        else
        {
            dialogueBox.SetActive(false);
            yield break;
        }
    }
    public void Transmitting()
    {
        Debug.Log("按钮1");
        if(sceneToGo!=null)
            EventHandler.CallTransitionEvent(sceneToGo, positionToGo);
        dialogueBox.SetActive(false);

    }
    public void Close()
    {
        Debug.Log("按钮2");
        dialogueBox.SetActive(false);
    }
}
