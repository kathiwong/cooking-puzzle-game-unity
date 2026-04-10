using UnityEngine;
using TMPro;

[System.Obsolete("Use VersusPlayerController instead. This legacy script is kept only to avoid compile errors.", false)]
public class PlayerController : MonoBehaviour
{
    [Header("Legacy References")]
    public TextMeshProUGUI scoreText;

    private void Reset()
    {
        Debug.LogWarning("PlayerController is a legacy script. Use VersusPlayerController for versus gameplay.");
    }
}
