using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Health : MonoBehaviour
{
    TextMeshProUGUI tmp;
    [SerializeField] PlayerController player;

    void Start() {
        tmp = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        tmp.text = "HP: " + player.player.health.ToString();
    }
}
