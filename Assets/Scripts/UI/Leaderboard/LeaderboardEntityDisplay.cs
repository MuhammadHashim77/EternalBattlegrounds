using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    public ulong ClientId { get; private set; }

    private FixedString32Bytes playerName;

    public void Initialise(ulong clientId, FixedString32Bytes playerName)
    {
        ClientId = clientId;
        this.playerName = playerName;

        UpdateText();
    }

    private void UpdateText()
    {
        displayText.text = $"{ClientId + 1}. {playerName}";
    }
}
