using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
    public static GameMgr Instance { get; private set; }

    [SerializeField] private GameObject weaponUI;
    public Slider mySlider;

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

    void Start()
    {
        if (mySlider != null)
        {
            mySlider.maxValue = 100f; // スライダーの最大値も設定しておくと安心です
            mySlider.value = 100f;
        }
    }

    void Update()
    {
        HPSlider();
    }

    void HPSlider()
    {
        // PlayerSatus.Instance と mySlider の両方が null でないか確認
        if (PlayerSatus.Instance != null && mySlider != null)
        {
            mySlider.value = PlayerSatus.Instance.CurrentHP;
        }
    }
}