using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    private static SkyboxManager singleton;
    void Awake(){
        singleton = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }
    public IEnumerator LerpExposure(float start, float end, float speed)
    {
        float t = 0;
        while (t < 1)
        {
            t += speed * Time.deltaTime;
            RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(start, end, t));
            yield return null;
        }
    }
    public IEnumerator LerpColor(Color start, Color end, float speed)
    {
        float t = 0;
        while (t < 1)
        {
            t += speed * Time.deltaTime;
            RenderSettings.skybox.SetColor("_SkyTint", Color.Lerp(start, end, t));
            yield return null;
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
    public float exposureSpeed, colorSpeed;
    public static void Darken()
    {
        singleton.StartCoroutine(singleton.LerpExposure(RenderSettings.skybox.GetFloat("_Exposure"), 0, singleton.exposureSpeed));
    }
    public static void Lighten()
    {
        singleton.StartCoroutine(singleton.LerpExposure(RenderSettings.skybox.GetFloat("_Exposure"), 1f, singleton.exposureSpeed));
    }
    public static void TurnToColor(Color color)
    {
        singleton.StartCoroutine(singleton.LerpColor(RenderSettings.skybox.GetColor("_SkyTint"), color, singleton.colorSpeed));
    }
}
