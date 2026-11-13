using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
    public static GameMgr Instance { get; private set; }

    [SerializeField] private Slider hpSlider;
    [SerializeField] private GameObject fillObject; // Sliderの「Fill」を指定
    [SerializeField] private GameObject weaponUI;

    // GameObject ではなく Text コンポーネント型で直接受け取る
    [SerializeField] private Text bulletCountText;

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
    }

    private void Start()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = 100f;
            hpSlider.value = 100f;
        }
    }

    private void Update()
    {
        UpdateHPSlider();
        UpdateBulletCount();
    }

    private void UpdateHPSlider()
    {
        if (PlayerSatus.Instance != null && hpSlider != null)
        {
            float currentHp = PlayerSatus.Instance.CurrentHP;
            hpSlider.value = currentHp;

            // HPが0より大きい時だけ緑色のゲージを表示する
            if (fillObject != null)
            {
                fillObject.SetActive(currentHp > 0);
            }
        }
    }

    private void UpdateBulletCount()
    {
        // プレイヤーが存在し、テキストUIが割り当てられている場合のみ更新
        if (K04_Player.Instance != null && bulletCountText != null)
        {
            bulletCountText.text = K04_Player.Instance.bulletcount.ToString();
        }
    }
}