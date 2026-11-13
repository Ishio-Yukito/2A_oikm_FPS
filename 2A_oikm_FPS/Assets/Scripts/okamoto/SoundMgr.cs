using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundMgr : MonoBehaviour
{
    public static SoundMgr Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource bgmSource2;
    [SerializeField] private AudioSource bgmSource3;
    [SerializeField] private AudioSource seSource;

    [Header("Default Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float bgm2Volume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float bgm3Volume = 1f;


    [Range(0f, 1f)]
    [SerializeField] private float seVolume = 4f;



    [Header("BGM Clips")]
    [SerializeField] private List<AudioClip> bgmClips = new List<AudioClip>();

    [Header("SE Clips")]
    [SerializeField] private List<AudioClip> seClips = new List<AudioClip>();

    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> seDictionary = new Dictionary<string, AudioClip>();

    private Coroutine bgmCoroutine;
    private void Start()
    {
        /*
            //途中に音量を変える
            SoundManager.Instance.SetBGMVolume(0.5f);
            SoundManager.Instance.SetSEVolume(0.8f);
            //BGM・SEの再生 (拡張子はいらない)
            SoundManager.Instance.PlayBGM("ファイル名");
            SoundManager.Instance.PlayBGM2("ファイル名");
            SoundManager.Instance.PlaySE("ファイル名");
        */
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudioSources();
        RegisterClips();
    }

    private void InitializeAudioSources()
    {
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        if (bgmSource2 == null)
            bgmSource2 = gameObject.AddComponent<AudioSource>();

        if (seSource == null)
            seSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        bgmSource.loop = false;
        bgmSource.volume = bgmVolume;

        bgmSource2.playOnAwake = false;
        bgmSource2.loop = false;
        bgmSource2.volume = bgm2Volume;

        seSource.playOnAwake = false;
        seSource.loop = false;
        seSource.volume = seVolume;
    }

    private void RegisterClips()
    {
        bgmDictionary.Clear();
        seDictionary.Clear();

        foreach (AudioClip clip in bgmClips)
        {
            if (clip != null && !bgmDictionary.ContainsKey(clip.name))
            {
                bgmDictionary.Add(clip.name, clip);
            }
        }

        foreach (AudioClip clip in seClips)
        {
            if (clip != null && !seDictionary.ContainsKey(clip.name))
            {
                seDictionary.Add(clip.name, clip);
            }
        }
    }

    //==============================
    // BGM
    //==============================

    public void PlayBGM(string bgmName, bool loop = true, float volume = -1f)
    {
        if (!bgmDictionary.TryGetValue(bgmName, out AudioClip clip))
        {
            Debug.LogWarning($"BGM '{bgmName}' が見つかりません。");
            return;
        }

        StopBGM();

        if (volume >= 0f)
            bgmVolume = Mathf.Clamp01(volume);

        bgmSource.volume = bgmVolume;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayBGM(AudioClip clip, bool loop = true, float volume = -1f)
    {
        if (clip == null)
            return;

        StopBGM();

        if (volume >= 0f)
            bgmVolume = Mathf.Clamp01(volume);

        bgmSource.volume = bgmVolume;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayBGMSequence(string introName, string loopName, float volume = -1f)
    {
        if (!bgmDictionary.TryGetValue(introName, out AudioClip intro))
        {
            Debug.LogWarning($"Intro BGM '{introName}' がありません。");
            return;
        }

        if (!bgmDictionary.TryGetValue(loopName, out AudioClip loop))
        {
            Debug.LogWarning($"Loop BGM '{loopName}' がありません。");
            return;
        }

        StopBGM();

        if (volume >= 0f)
            bgmVolume = Mathf.Clamp01(volume);

        bgmCoroutine = StartCoroutine(CoPlayBGMSequence(intro, loop));
    }

    public void PlayBGMSequence(AudioClip intro, AudioClip loop, float volume = -1f)
    {
        if (intro == null || loop == null)
            return;

        StopBGM();

        if (volume >= 0f)
            bgmVolume = Mathf.Clamp01(volume);

        bgmCoroutine = StartCoroutine(CoPlayBGMSequence(intro, loop));
    }

    private IEnumerator CoPlayBGMSequence(AudioClip intro, AudioClip loop)
    {
        bgmSource.volume = bgmVolume;
        bgmSource.clip = intro;
        bgmSource.loop = false;
        bgmSource.Play();

        yield return new WaitForSeconds(intro.length);

        bgmSource.clip = loop;
        bgmSource.loop = true;
        bgmSource.Play();

        bgmCoroutine = null;
    }

    public void StopBGM()
    {
        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
        }

        bgmSource.Stop();
        bgmSource.clip = null;
    }
    //==============================
    // SE
    //==============================

    public void PlaySE(string seName, float volume = -1f)
    {
        if (!seDictionary.TryGetValue(seName, out AudioClip clip))
        {
            Debug.LogWarning($"SE '{seName}' が見つかりません。");
            return;
        }

        if (volume < 0f)
            volume = seVolume;

        seSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void PlaySE(AudioClip clip, float volume = -1f)
    {
        if (clip == null)
            return;

        if (volume < 0f)
            volume = seVolume;

        seSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    //==============================
    // 音量
    //==============================

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        seSource.volume = seVolume;
    }

    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    public float GetSEVolume()
    {
        return seVolume;
    }

    //==============================
    // Inspectorで変更した値を反映
    //==============================

    private void OnValidate()
    {
        bgmVolume = Mathf.Clamp01(bgmVolume);
        seVolume = Mathf.Clamp01(seVolume);

        if (bgmSource != null)
            bgmSource.volume = bgmVolume;

        if (seSource != null)
            seSource.volume = seVolume;
    }

    //==============================
    // フェード
    //==============================

    private Coroutine fadeCoroutine;

    public void FadeBGMVolume(float targetVolume, float duration)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CoFadeBGM(targetVolume, duration));
    }

    private IEnumerator CoFadeBGM(float targetVolume, float duration)
    {
        targetVolume = Mathf.Clamp01(targetVolume);

        float startVolume = bgmSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, t);

            yield return null;
        }

        bgmSource.volume = targetVolume;
        bgmVolume = targetVolume;

        fadeCoroutine = null;
    }


    // ===== BGM2 =====
    public void PlayBGM2(string bgmName, bool loop = true, float volume = -1f)
    {
        if (!bgmDictionary.TryGetValue(bgmName, out AudioClip clip)) return;
        if (volume >= 0f) bgm2Volume = Mathf.Clamp01(volume);
        bgmSource2.clip = clip;
        bgmSource2.loop = loop;
        bgmSource2.volume = bgm2Volume;
        bgmSource2.Play();
    }

    public void StopBGM2()
    {
        bgmSource2.Stop();
        bgmSource2.clip = null;
    }

    public void SetBGM2Volume(float volume)
    {
        bgm2Volume = Mathf.Clamp01(volume);
        bgmSource2.volume = bgm2Volume;
    }

    public float GetBGM2Volume() => bgm2Volume;

    // ===== BGM3 =====
    public void PlayBGM3(string bgmName, bool loop = true, float volume = -1f)
    {
        if (!bgmDictionary.TryGetValue(bgmName, out AudioClip clip))
            return;

        if (volume >= 0f)
            bgm3Volume = Mathf.Clamp01(volume);

        bgmSource3.clip = clip;
        bgmSource3.loop = loop;
        bgmSource3.volume = bgm3Volume;
        bgmSource3.Play();
    }

    public void StopBGM3()
    {
        bgmSource3.Stop();
        bgmSource3.clip = null;
    }

    public void SetBGM3Volume(float volume)
    {
        bgm3Volume = Mathf.Clamp01(volume);
        bgmSource3.volume = bgm3Volume;
    }

    public float GetBGM3Volume()
    {
        return bgm3Volume;
    }

}

