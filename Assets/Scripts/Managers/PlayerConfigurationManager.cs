using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;

    [SerializeField] private int maxPlayers = 2;

    public static PlayerConfigurationManager Instance {get; private set;}

    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            playerConfigs = new List<PlayerConfiguration>();
        }
        else {
            Destroy(gameObject);
        }
    }

    public void SetClass(int index, PlayerClass playerClass) {
        playerConfigs[index].playerClass = playerClass;
    }

    public void ReadyPlayer(int index) {
        playerConfigs[index].IsReady = true;
        if(playerConfigs.Count == maxPlayers && playerConfigs.All(p => p.IsReady == true)) {
            LevelManager.instance.LoadScene("MainScene");
        }
    }

    public void HandlePlayerJoin(PlayerInput playerInput) {
        if(!playerConfigs.Any(p => p.PlayerIndex == playerInput.playerIndex)) {
            playerInput.transform.SetParent(transform);
            playerConfigs.Add(new PlayerConfiguration(playerInput));
        }
    }
}

public class PlayerConfiguration {

    public PlayerConfiguration(PlayerInput playerInput) {
        PlayerIndex = playerInput.playerIndex;
        Input = playerInput;
    }

    public PlayerInput Input {get; set;}
    
    public int PlayerIndex {get; set;}
    
    public bool IsReady {get; set;}

    public PlayerClass playerClass;

}
