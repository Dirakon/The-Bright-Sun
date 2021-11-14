using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake(){
        allowed = true;
    }
    public ParticleSystem allowedParticle;
    public ActionOnE action;
    public GameObject objectToShootAt;
    public AudioSource source;
    public AudioClip laserSound;
    public static void Allow(){
        allowed = true;
    }
    void Start()
    {
        action = GetComponent<ActionOnE>();
        action.action+=Clicked;
        p1.Stop();
        p2.Stop();
    }
    void Clicked(){
        if (!allowed)
            return;
        
        Player.RegenHPBy(20);
        StartCoroutine(shootAt(objectToShootAt.transform));
        allowed=false;
    }
    public GameObject theLaserItself;
    public ParticleSystem p1,p2;
    IEnumerator shootAt(Transform point, float speed = 0.5f){
        source.clip=laserSound;
        source.loop=true;
        source.Play();
        theLaserItself.transform.LookAt(point);
        theLaserItself.SetActive(true);
        p1.Play();
        p2.Play();
        Vector3 startVector = new Vector3(1,1,0);
        Vector3 endVector = new Vector3(1,1,(point.position-theLaserItself.transform.position).magnitude/2);
        float t = 0;
        while (t < 1){
            t += Time.deltaTime*speed;
            theLaserItself.transform.localScale = Vector3.Lerp(startVector,endVector,t);
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        theLaserItself.transform.localScale = new Vector3(0,0,0);
        p1.Stop();
        p2.Stop();
        Sun.NextStage();
        source.Stop();
    }
    public static bool allowed;
    // Update is called once per frame
    void Update()
    {
        if (allowedParticle.isPlaying){
            if (!allowed)
                allowedParticle.Stop();
        }else{
            if (allowed){
                allowedParticle.Play();
            }
        }
        action.popUpText = allowed?"Press E to activate the laser":"Laser is not ready";
    }
}
