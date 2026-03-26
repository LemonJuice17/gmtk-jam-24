using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public Slider SFXSlider;
    public Slider MusicSlider;

    public AudioMixer AudioMixer;
    
    void Start()
    {
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");

        SetSFXVolume();
        SetMusicVolume();
    }

    public void SetSFXVolume() 
    {
        AudioMixer.SetFloat("SFXVolume", SFXSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", SFXSlider.value);
    } 
    public void SetMusicVolume()
    {
        AudioMixer.SetFloat("MusicVolume", MusicSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", MusicSlider.value);
    } 
}