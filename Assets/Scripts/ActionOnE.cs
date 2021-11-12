using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ActionOnE : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public Action action;
    public string popUpText = "Press E to go to hell.";
    public float dist = 2f;
    bool active = false;
    //Upon collision with another GameObject, this GameObject will reverse direction
    public void ActivatePopUp(){
        active = true;
        PopUp.singleton.tmpro.text = popUpText;
    }
    public void DeactivatePopUp(){
        active = false;
        PopUp.singleton.tmpro.text = "";
    }
    public void ActivateAction(){
        action?.Invoke();
    }
    // Update is called once per frame
    void Update()
    {
        if (active)ActivatePopUp();
    }
}
