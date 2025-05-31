using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public BossController boss;
    private Image BossHealthBarImage;

    void Start()
    {
        BossHealthBarImage = GetComponent<Image>();
        BossHealthBarImage.fillAmount = 1;
    }

    void Update()
    {
        BossHealthBarImage.fillAmount = (float)boss.GetCurrentHealth() / 300;
    }
}