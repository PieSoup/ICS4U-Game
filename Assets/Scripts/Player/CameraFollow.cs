using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
// This class gets the player transform and sets the cinemachine camera follow parameter to it
public class CameraFollow : MonoBehaviour
{
    CinemachineVirtualCamera vcam;
    PlayerController[] players;

    [SerializeField] int index;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {   
        // Set the follow parameter to the player transform
        if (players == null || players.Length == 0 || players.Length == index)
        {
            players = FindObjectsOfType<PlayerController>();
        }
        else if (vcam.Follow == null)
        {
            vcam.Follow = players[0].transform;
        }
    }
}
