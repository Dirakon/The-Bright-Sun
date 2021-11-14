using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PopUp : MonoBehaviour
{
    public static PopUp singleton;
    public TextMeshProUGUI tmpro,secondTmPro;
    [SerializeField]public Image blackScreen;
    // Start is called before the first frame update
    void Start()
    {
    }
    public static float levelOfSunness;
    float speed = 1f;
    IEnumerator BlackScreenOn()
    {
        blackScreen.gameObject.SetActive(true);
        Color startColor = blackScreen.color;
        Color endColor = blackScreen.color;
        endColor.a=1f;
        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {

            blackScreen.color = Color.Lerp(startColor,endColor,t);
            yield return null;
        }
    }

    IEnumerator BlackScreenOff()
    {
        Color startColor = blackScreen.color;
        Color endColor = blackScreen.color;
        endColor.a=0f;

        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {

            blackScreen.color = Color.Lerp(startColor,endColor,t);
            yield return null;
        }
        blackScreen.gameObject.SetActive(false);
    }
    public void sunScreenOn()
    {
        StartCoroutine(BlackScreenOn());
    }
    public void sunScreenOff()
    {

        StartCoroutine(BlackScreenOff());
    }
    void Awake()
    {
        levelOfSunness=  0f;
        singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        Color color = blackScreen.color;
        color.a = levelOfSunness;
        blackScreen.color = color;
       // if (RandomEventManager.eventAllowed)
       //     secondTmPro.text = "";
    }
}
