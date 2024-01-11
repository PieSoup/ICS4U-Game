using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;

// This class simply controls an index for the amount of players there are in the scene, and updates whenever one joins
public class JoinManager : MonoBehaviour
{
    public static int currentPlayerIndex = -1;

    private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] private List<LayerMask> playerLayers = new List<LayerMask>();

    private PlayerInputManager playerInputManager;

    private void Awake() {
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable() {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable() {
        playerInputManager.onPlayerLeft += AddPlayer;
    }

    public void AddPlayer(PlayerInput player) {
        currentPlayerIndex += 1;

        Transform playerParent = player.transform.parent;
        players.Add(player);

        int layerToAdd = (int) Mathf.Log(playerLayers[players.Count - 1].value, 2);

        playerParent.GetComponentInChildren<CinemachineVirtualCamera>().gameObject.layer = layerToAdd;
        playerParent.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;

        foreach(Parallax parallax in playerParent.GetComponentsInChildren<Parallax>()) {
            parallax.gameObject.layer = layerToAdd;
        }
    }
}
