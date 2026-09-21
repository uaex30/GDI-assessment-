using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip introClip;
    public AudioClip normalClip;

    void Start()
    {
        musicSource.clip = introClip;
        musicSource.Play();
        StartCoroutine(SwitchToLoop());
    }

    IEnumerator SwitchToLoop()
    {
        yield return new WaitForSeconds(
            Mathf.Min(introClip.length, 3f)
        );

        musicSource.clip = normalClip;
        musicSource.loop = true;
        musicSource.Play();
    }
}