using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// This class simply controls an index for the amount of players there are in the scene, and updates whenever one joins
public class JoinManager : MonoBehaviour
{
    public static int currentPlayerIndex = -1;

    private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] private List<LayerMask> playerLayers = new List<LayerMask>();

    [SerializeField] private Canvas joinCanvas;
    [SerializeField] private TextMeshProUGUI joinText;

    private PlayerInputManager playerInputManager;

    private void Awake() {
        playerInputManager = GetComponent<PlayerInputManager>();
        joinText.text = "Player 1 press any button to join.";
    }

    private void OnEnable() {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable() {
        playerInputManager.onPlayerLeft += AddPlayer;
    }

    public void AddPlayer(PlayerInput player) {
        currentPlayerIndex += 1;
        if(currentPlayerIndex == 0) {
            joinText.text = "Player 2 press any button to join";
        }
        else {
            joinCanvas.gameObject.SetActive(false);
        }
        Transform playerParent = player.transform.parent;
        players.Add(player);

        int layerToAdd = (int) Mathf.Log(playerLayers[players.Count - 1].value, 2);

        playerParent.GetComponentInChildren<CinemachineVirtualCamera>().gameObject.layer = layerToAdd;
        playerParent.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;

        foreach(Parallax parallax in playerParent.GetComponentsInChildren<Parallax>()) {
            parallax.gameObject.layer = layerToAdd;
            foreach(Transform child in parallax.transform) {
                child.gameObject.layer = layerToAdd;
            }
        }
    }
}
