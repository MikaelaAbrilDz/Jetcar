using TMPro;
using UnityEngine;

public class StarManager : MonoBehaviour
{
    public static StarManager Instance;

    [SerializeField] private TextMeshProUGUI starText;

    private int stars = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        UpdateUI();
    }

    public void AddStar()
    {
        stars++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        starText.text = stars + "/32";
    }
}