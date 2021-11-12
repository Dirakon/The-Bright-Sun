using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class Sun : MonoBehaviour
{
    public int curPhase = 0;
    public PhaseInfo[] phases;
    public Light[] mainLights;
    public Light[] secondaryLights;
    public Transform pointToLookAt;
    public Transform sphereCenter;
    public float rotSpeed = 50f;
    public float cheatMoveSpeed = 10f;
    public float sphereRadius;
    // Start is called before the first frame update
    void Start()
    {
    }
    public IEnumerator RotateHorizontally()
    {
        Vector3 levitatedCenter = sphereCenter.position;
        levitatedCenter.y = transform.position.y;
        float randRotation = Random.Range(-180, 180);
        float sign = Mathf.Sign(randRotation);
        randRotation = Mathf.Abs(randRotation);
        while (randRotation > 0)
        {
            float mov = Time.deltaTime * rotSpeed;
            if (mov > randRotation)
                mov = randRotation;
            randRotation -= mov;
            transform.RotateAround(levitatedCenter,Vector3.up,mov*sign);
            yield return null;
        }
    }
    public IEnumerator RotateVertically()
    {

        float randRotation = Random.Range(-180, 180);
       // float saveMinY = minY + 10f;

        float sign = Mathf.Sign(randRotation);
        randRotation = Mathf.Abs(randRotation);
        Vector3 lastPos = transform.position;
        Vector3 back = Vector3.Cross(Vector3.up,sphereCenter.position-transform.position);
        while (randRotation > 0 && ((transform.position-lastPos).y >= 0 ||transform.position.y >= minY))
        {
            float mov = Time.deltaTime * rotSpeed;
            if (mov > randRotation)
                mov = randRotation;
            randRotation -= mov;
            lastPos = transform.position;
            transform.RotateAround(sphereCenter.position,back,mov*sign);
            yield return null;
        }
        yield return null;
    }
    public float minY;
    public IEnumerator GetAboveThePoint(Vector3 point)
    {
        point.y = Mathf.Sqrt(sphereRadius * sphereRadius - point.x * point.x -
        point.z * point.z);
        float dist = (point -transform.position).magnitude;
        Vector3 startPos = transform.position;
        for (float t = 0; t < dist;){
            t+=cheatMoveSpeed*Time.deltaTime;
            transform.position = Vector3.Lerp(startPos,point,t/dist);
            
        yield return null;
        }
    }
    void Awake()
    {
        StartCoroutine(SunAI());
        Vector3 pos = Random.onUnitSphere*sphereRadius;
        do{
        pos.y=Mathf.Abs(pos.y);
        if (pos.y > minY)
            break;
        pos = Random.onUnitSphere*sphereRadius;
        }while (true);
        transform.position = pos;
        singleton = this;
    }
    IEnumerator SunAI(){
        while (true){
            yield return RotateHorizontally();
            yield return RotateVertically();
         //   yield return GetAboveThePoint(sphereCenter.position + new Vector3(10,10,10));
        }
    }
    private static Sun singleton;
    public static void NextStage()
    {
        singleton.curPhase++;
        if (singleton.curPhase == singleton.phases.Length)
        {
            // game is won
        }
        else
        {
            singleton.StartCoroutine(singleton.nextStageAnimation());
        }
    }
    public float scalingSpeed = 0.5f;
    IEnumerator nextStageAnimation()
    {
        SkyboxManager.Darken();
        SkyboxManager.TurnToColor(Color.black);
        foreach (var light in secondaryLights)
        {
            light.gameObject.SetActive(false);
        }
        foreach (var light in mainLights)
        {
            light.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(5f);
        SkyboxManager.Lighten();
        SkyboxManager.TurnToColor(phases[curPhase].skyboxColor);
        foreach (var light in secondaryLights)
        {
            light.gameObject.SetActive(true);
        }
        foreach (var light in mainLights)
        {
            light.gameObject.SetActive(true);
        }
        float newScale = phases[curPhase].scale;
        Vector3 endScale = (transform.localScale / curScale) * newScale;
        Vector3 startScale = transform.localScale;
        float startLight = secondaryLights[0].range;
        float endLight = (startLight / curScale) * newScale;
        for (float t = 0; t < 1; t += Time.deltaTime * scalingSpeed)
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            float curLight = Mathf.Lerp(startLight, endLight, t);
            foreach (var light in secondaryLights)
            {
                light.range = curLight;
            }
            yield return null;
        }
    }
    // Update is called once per frame
    Color prevMain, prevSec;
    float curScale = 1;
    void Update()
    {
        transform.LookAt(pointToLookAt, Vector3.up);
        if ((prevSec == phases[curPhase].secondaryColor && prevMain == phases[curPhase].mainColor))
            return;
        prevSec = phases[curPhase].secondaryColor;
        prevMain = phases[curPhase].mainColor;
        foreach (var light in mainLights)
        {
            light.color = phases[curPhase].mainColor;
        }
        foreach (var light in secondaryLights)
        {
            light.color = phases[curPhase].secondaryColor;
        }
    }
}
