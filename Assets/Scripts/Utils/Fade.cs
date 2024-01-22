using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] float duration;

    bool gameEnded;

    void Update() {
        if(player.player.health <= 0) {
            DoFade();
        }
    }

    private void DoFade() {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup.alpha < 1) {
            canvasGroup.alpha += Time.deltaTime / duration;
        }
        else if(!gameEnded){
            gameEnded = true;
            StartCoroutine(GameManager.instance.EndGame(player.player.playerIndex));
        }
    }
}
