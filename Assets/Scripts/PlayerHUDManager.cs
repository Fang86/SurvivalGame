using UnityEngine;
using TMPro;

public class PlayerHUDManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI killsText;

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = $"{current}/{max}";
    }

    public void UpdateKills(int kills)
    {
        if (killsText != null)
            killsText.text = $"{kills} kills";
    }
}
