using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
    public static GameMgr Instance { get; private set; }

    [SerializeField] private Slider hpSlider;
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
        if (PlayerContoroller.Instance != null && hpSlider != null)
        {
            hpSlider.value = PlayerContoroller.Instance.Hp;
        }
    }

    private void UpdateBulletCount()
    {
        // プレイヤーが存在し、テキストUIが割り当てられている場合のみ更新
        if (PlayerContoroller.Instance != null && bulletCountText != null)
        {
            bulletCountText.text = PlayerContoroller.Instance.bulletcount.ToString();
        }
    }
}