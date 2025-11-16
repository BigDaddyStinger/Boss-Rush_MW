using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] float fadeSpeed = 1;
    [SerializeField] AudioClip recordScratch;
    AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void FadeOut()
    {
        StartCoroutine(HandleFadeOut());
    }

    IEnumerator HandleFadeOut()
    {
        while(source.volume > 0)
        {
            source.volume -= Time.deltaTime * fadeSpeed;
            yield return new WaitForEndOfFrame();
        }    
    }

    public void AbruptlyStop()
    {
        source.Stop();
        source.loop = false;
        source.clip = recordScratch;
        source.Play();
    }
}
