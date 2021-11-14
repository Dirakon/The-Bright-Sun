using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    public float mainSpeed = 100.0f; //regular speed
    public float _mouseSensitivity = 20f; //How sensitive it with mouse
    public Camera camera;
    public Rigidbody rigidbody;
    private static Player singleton;
    IEnumerator actualCameraShake(Quaternion startQuaternion, Quaternion endQuaternion)
    {
        const float speed = 20f;
        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {
            singleton.camera.transform.rotation = Quaternion.Lerp(startQuaternion, endQuaternion, t);
            yield return null;
        }
        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {
            singleton.camera.transform.rotation = Quaternion.Lerp(endQuaternion, startQuaternion, t);
            yield return null;
        }
        singleton.camera.transform.rotation = startQuaternion;
    }
    public static void CameraShake(float value)
    {
        Quaternion startQuaternion = singleton.camera.transform.rotation;
        singleton.camera.transform.RotateAround(singleton.camera.transform.position, Vector3.right, value);
        Quaternion endQuaternion = singleton.camera.transform.rotation;
        singleton.camera.transform.RotateAround(singleton.camera.transform.position, Vector3.right, -value);
        singleton.StartCoroutine(singleton.actualCameraShake(startQuaternion, endQuaternion));
    }
    void Awake()
    {
        player = sourcesToMute[0];
        singleton = this;
    }
    private float xRotation = 0f;
    [SerializeField] private float _minCameraview = -70f, _maxCameraview = 80f;
    // Start is called before the first frame update
    void Start()
    {

        RenderHP();
    }
    public void HideMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void ShowMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }
    public static void CancelEverything(bool showMouse = true)
    {
        singleton.previousAction?.DeactivatePopUp();
        singleton.previousAction = null;
        singleton.cancelEverythingFlag = true;
        if (showMouse)
            singleton.ShowMouse();
    }
    IEnumerator safeAllow()
    {
        yield return new WaitForSeconds(0.1f);
        singleton.cancelEverythingFlag = false;
    }
    public static void AllowEverything()
    {
        singleton.StartCoroutine(singleton.safeAllow());
    }
    bool cancelEverythingFlag = false;
    ActionOnE previousAction = null;
    public LayerMask actionELayerMask;
    public GameObject UI_panel;
    public bool menu = false;
    public static float slider_sensitivity_value = 0.2f;
    public Slider slider;
    public AudioSource[] sourcesToMute;
    public void SliderChanged()
    {
        slider_sensitivity_value = slider.value;
    }
    public void GameOverClicked()
    {
        Application.Quit();
    }
    public void ResumeClicked()
    {

        menu = false;
        UI_panel.SetActive(false);
        AllowEverything();
            foreach (var source in sourcesToMute)
            source.pitch = 1;
        Time.timeScale = 1f;
        PopUp.singleton.blackScreen.gameObject.SetActive(true);
    }
    // Update is called once per frame
    //Upon collision with another GameObject, this GameObject will reverse direction
    float hp = 100;
   // public TextMeshProUGUI hpText;
    void RenderHP(){
        PopUp.singleton.secondTmPro.text = ((int)hp).ToString();
    }
    public float sunnessVelocityAttaked = 2f,sunnessVelocityNormal = -0.5f;
    public static void getSunned(float sunnesFactor=1){
        singleton.hp -= PopUp.levelOfSunness*Time.deltaTime*Sun.tagToDamage["SunBurn"] ;
       singleton.RenderHP();
        if (singleton.hp <= 0){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex,LoadSceneMode.Single);
        }
        if (PopUp.levelOfSunness > 1)
            PopUp.levelOfSunness = 1;
        PopUp.levelOfSunness += Time.deltaTime*singleton.sunnessVelocityAttaked*sunnesFactor;
    }
    public static void getUnsunned(){
        PopUp.levelOfSunness += Time.deltaTime*singleton.sunnessVelocityNormal;
        if (PopUp.levelOfSunness < 0)
            PopUp.levelOfSunness = 0;
    }
    private void OnTriggerStay(Collider other)
    {
        if (LaserTower.allowed)
            return;
        getSunned(2f);
        hp -= Time.deltaTime*Sun.tagToDamage[other.gameObject.tag];    
        if (hp <= 0){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex,LoadSceneMode.Single);
        }
        RenderHP();
    }
    public AudioSource walk;
    public static void RegenHPBy(float reg){
        singleton.hp +=reg;
    }
    void Update()
    {
        if (LaserTower.allowed){
            PopUp.singleton.secondTmPro.text = "Activate the laser tower!";
        }else{
            RenderHP();
        }
        _mouseSensitivity = slider_sensitivity_value * 40f;
        slider.value = slider_sensitivity_value;
        if (menu && Input.GetKeyDown(KeyCode.Escape))
        {
            ResumeClicked();
            return;
        }
        if (cancelEverythingFlag)
            return;
        
        if (!menu && Input.GetKeyDown(KeyCode.Escape))
        {
        PopUp.singleton.blackScreen.gameObject.SetActive(false);
            menu = true;
            UI_panel.SetActive(true);
            CancelEverything();
            Time.timeScale = 0f;
            foreach (var source in sourcesToMute)
            source.pitch = 0;
            return;
        }
        camera.transform.localPosition = new Vector3(0, 0, 0);
        HideMouse();
        getUnsunned();
        RaycastHit hit;
        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        ActionOnE action = null;
        if (Physics.Raycast(ray, out hit, 9999f, actionELayerMask))
        {

            action = hit.collider.GetComponent<ActionOnE>();
            if (action != null && (action.transform.position - camera.transform.position).magnitude >=
            action.dist)
            {
                action = null;
            }
            // Do something with the object that was hit by the raycast.
        }
        if (previousAction != action)
        {
            previousAction?.DeactivatePopUp();
            action?.ActivatePopUp();
        }
        previousAction = action;
        if (Input.GetKeyDown(KeyCode.E))
        {
            action?.ActivateAction();
            rigidbody.velocity = new Vector3(0, 0, 0);
        }
        else
        {
            //Get Mouse position Input
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity; //changed this line.
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity; //changed this line.
                                                                         //Rotate the camera based on the Y input of the mouse
            xRotation -= mouseY;
            //clamp the camera rotation between 80 and -70 degrees
            xRotation = Mathf.Clamp(xRotation, _minCameraview, _maxCameraview);

            camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            //Rotate the player based on the X input of the mouse
            transform.Rotate(Vector3.up * (mouseX * 1.5f));


            //Keyboard commands
            rigidbody.velocity = GetBaseInput() * mainSpeed *(1-PopUp.levelOfSunness)* (Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f);
            if (rigidbody.velocity.sqrMagnitude > 0.001){
                if (!walk.isPlaying){
                    walk.Play();
                }
            }else{
                if (walk.isPlaying){
                    walk.Stop();
                }
            }
        }
    }
    public static AudioSource player;
    private Vector3 GetBaseInput()
    {
        Vector3 right = camera.transform.right;
        right.y = 0;
        Vector3 forward = camera.transform.forward;
        forward.y = 0;
        return (forward * Input.GetAxis("Vertical") +
        right * Input.GetAxis("Horizontal")).normalized;
    }
}

