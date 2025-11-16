using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] Animator screenFade;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void RestartLevel()
    {
        StartCoroutine(HandleRestart());
        FindObjectOfType<BackgroundMusic>().AbruptlyStop();
    }

    IEnumerator HandleRestart()
    {
        screenFade.SetTrigger("fade to black");
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void GoToNextLevel()
    {
        StartCoroutine(HandleNextLevel());
        FindObjectOfType<BackgroundMusic>().FadeOut();
    }

    IEnumerator HandleNextLevel()
    {
        screenFade.SetTrigger("fade to black");
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
    }

    public void ActivateHitStop(float time)
    {
        StartCoroutine(HandleHitStop(time));
    }

    IEnumerator HandleHitStop(float time)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1;
    }

    public void ActivateCameraShake()
    {
        StartCoroutine(HandleCameraShake());
    }

    IEnumerator HandleCameraShake()
    {
        var cam = FindObjectOfType<CinemachineVirtualCamera>();
        //cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 3;
        var noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        float amount = 10;
        while(amount > 0)
        {
            amount -= Time.deltaTime * 10;
            noise.m_AmplitudeGain = amount;
            yield return new WaitForEndOfFrame();
        }
    }

    public void DesaturateScreen()
    {
        StartCoroutine(HandleDesaturation());
    }

    IEnumerator HandleDesaturation()
    {
        var volume = FindObjectOfType<Volume>();
        ColorAdjustments colors;
        
        volume.profile.TryGet(out colors);

        if(colors != null)
        {
            var originalValue = colors.saturation.value;
            colors.saturation.value = -100;

            while(colors.saturation.value <= originalValue)
            {
                colors.saturation.value += Time.deltaTime * 100;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void ChromaticAberration()
    {
        StartCoroutine(HandleChromaticAberration());
    }

    IEnumerator HandleChromaticAberration()
    {
        var volume = FindObjectOfType<Volume>();
        ChromaticAberration chromatic;

        volume.profile.TryGet(out chromatic);

        if (chromatic != null)
        {
            chromatic.intensity.value = 1;

            while (chromatic.intensity.value > 0)
            {
                chromatic.intensity.value -= Time.deltaTime * 2;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
