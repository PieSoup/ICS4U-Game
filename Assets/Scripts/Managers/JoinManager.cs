using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// This class simply controls an index for the amount of players there are in the scene, and updates whenever one joins
public class JoinManager : MonoBehaviour
{
    public static int currentPlayerIndex = -1;

    public void OnJoin() {
        currentPlayerIndex += 1;
    }
}
