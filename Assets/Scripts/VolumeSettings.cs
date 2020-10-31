using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal.Internal;

public class VolumeSettings : MonoBehaviour
{

    private AudioSource audioSrc;
    [SerializeField] Slider mSlider;
    private float musicVolume;


    // Start is called before the first frame update
    void Start()
    {
        audioSrc = Camera.main.GetComponent<AudioSource>();
        musicVolume = audioSrc.volume;
        mSlider.value = musicVolume;
    }

    // Update is called once per frame
    void Update()
    {
        audioSrc.volume = musicVolume;
    }

    public void SetVolume(float volume)
    {
        musicVolume = volume;
    }
}
