using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake(){
    }
    public ActionOnE action;
    void Start()
    {
        action = GetComponent<ActionOnE>();
        action.action+=Clicked;
    }
    void Clicked(){
        allowed=false;
    }
    public bool allowed = true;
    // Update is called once per frame
    void Update()
    {
        action.popUpText = allowed?"Press E to activate the laser":"Laser is not ready";
    }
}
