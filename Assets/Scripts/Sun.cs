using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Sun : MonoBehaviour
{
    public int curPhase=0;
    public PhaseInfo[] phases;
    public Light[] mainLights;
    public Light[] secondaryLights;
    public Transform pointToLookAt;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    IEnumerator nextStageAnimation(){
        SkyboxManager.Darken();
        SkyboxManager.TurnToColor(Color.black);
        yield return new WaitForSeconds(2f);
        SkyboxManager.Lighten();
        SkyboxManager.TurnToColor(phases[curPhase].skyboxColor);
        yield return new WaitForSeconds(1f);
    }
    public void NextStage(){

    }

    // Update is called once per frame
    Color prevMain,prevSec;
    void Update()
    {
        transform.LookAt(pointToLookAt,Vector3.up);
        if (prevSec == phases[curPhase].secondaryColor && prevMain == phases[curPhase].mainColor)
            return;
        prevSec=phases[curPhase].secondaryColor;
        prevMain=phases[curPhase].mainColor;
        foreach(var light in mainLights){
            light.color=phases[curPhase].mainColor;
        }
        foreach(var light in secondaryLights){
            light.color=phases[curPhase].secondaryColor;
        }
    }
}
