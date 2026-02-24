
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

   
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip stepSound;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip rollDiceSound;
    [SerializeField] private AudioClip kickSound;
    [SerializeField] private AudioClip marchSound;
    [SerializeField] private AudioClip finishSound;
    [SerializeField] private AudioClip hoverPieceSound;
    [SerializeField] private AudioClip endGameSound;


    [SerializeField] private AudioClip explosionFaceDetect;
    [SerializeField] private AudioClip eplosionWallDetect;

    private void Start()
    {
      
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);

      
        AddClickSoundToAllButtons();
    }

    public void ChangeMusicVolume(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void ChangeSFXVolume(float value)
    {
        sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    public void PlayClickSound()
    {
        if (clickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    private void AddClickSoundToAllButtons()
    {
       
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);


        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(PlayClickSound);
        }
    }
    public void PlayStepSound()
    {
        if (stepSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(stepSound);
        }
    }
    public void PlayRollSound()
    {
        if (rollSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(rollSound);
        }
    }
    public void PlayRollDice()
    {
        if (rollDiceSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(rollDiceSound);
        }
    }
    public void PlayKickSound()
    {
        if (kickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(kickSound);
        }
    }
    public void PlayMarchSound()
    {
        if (marchSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(marchSound);
        }
    }

    public void PlayFinishSound()
    {
        if (finishSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(finishSound);
        }
    }
    public void PlayHoverPieceSound()
    {
        if (hoverPieceSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hoverPieceSound);
        }
    }
    public void PlayExplosionFaceDetect()
    {
        if (explosionFaceDetect != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(explosionFaceDetect);
        }
    }
    public void PlayExplosionWallDetect()
    {
        if (eplosionWallDetect != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(eplosionWallDetect);
        }
    }
    public void PlayEndGameSound()
    {
        if (endGameSound != null && sfxSource == null)
        {
            sfxSource.PlayOneShot(endGameSound);
        }
    }
}
