using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PopUp : MonoBehaviour
{
    public static PopUp singleton;
    public TextMeshProUGUI tmpro,secondTmPro;
    [SerializeField]Image blackScreen;
    // Start is called before the first frame update
    void Start()
    {
        blackScreenOff();
    }
    float speed = 1f;
    IEnumerator BlackScreenOn()
    {
        blackScreen.gameObject.SetActive(true);
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 1);
        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {

            blackScreen.color = Color.Lerp(startColor,endColor,t);
            yield return null;
        }
    }

    IEnumerator BlackScreenOff()
    {
        Color startColor = new Color(0, 0, 0, 1);
        Color endColor = new Color(0, 0, 0, 0);


        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {

            blackScreen.color = Color.Lerp(startColor,endColor,t);
            yield return null;
        }
        blackScreen.gameObject.SetActive(false);
    }
    public void blackScreenOn()
    {
        StartCoroutine(BlackScreenOn());
    }
    public void blackScreenOff()
    {

        StartCoroutine(BlackScreenOff());
    }
    void Awake()
    {
        singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
       // if (RandomEventManager.eventAllowed)
       //     secondTmPro.text = "";
    }
}
