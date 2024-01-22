using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static GameState state;

    public Canvas endUI;
    public TextMeshProUGUI text;
    
    void Awake(){
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
        endUI.gameObject.SetActive(false);
    }

    public IEnumerator EndGame(int playerIndex) {
        
        switch(playerIndex) {
            case 0:
                text.text = "Player 2 wins!";
                break;
            case 1:
                text.text = "Player 1 wins!";
                break;
        }
        endUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        LevelManager.instance.LoadScene("StartMenu");
        endUI.gameObject.SetActive(false);
    }
}

public enum GameState {
    START,
    LOBBY,
    PLAY,
    END
}
