using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class Sun : MonoBehaviour
{
    public int curPhase = 0;
    public PhaseInfo[] phases;
    public Light[] mainLights;
    public Light[] secondaryLights;
    public Transform pointToLookAt;
    public Transform sphereCenter;
    public float sphereRadius;
    // Start is called before the first frame update
    void Start()
    {

        UpdateDamageDic();
        allAttacks = new Func<IEnumerator>[] { ManyRaysAttack, BigSlowLaser, SunBurn, StrikeFromAbove };
        StartCoroutine(SunAI());
    }
    public IEnumerator RotateHorizontally()
    {
        Vector3 levitatedCenter = sphereCenter.position;
        levitatedCenter.y = transform.position.y;
        float randRotation = UnityEngine.Random.Range(-45, 45);
        float sign = Mathf.Sign(randRotation);
        randRotation = Mathf.Abs(randRotation);
        while (randRotation > 0)
        {
            float mov = Time.deltaTime * conf.rotSpeed;
            if (mov > randRotation)
                mov = randRotation;
            randRotation -= mov;
            transform.RotateAround(levitatedCenter, Vector3.up, mov * sign);
            yield return null;
        }
        isInMove = false;
    }
    public IEnumerator RotateVertically()
    {

        float randRotation = UnityEngine.Random.Range(-180, 180);
        // float saveMinY = minY + 10f;

        float sign = Mathf.Sign(randRotation);
        randRotation = Mathf.Abs(randRotation);
        Vector3 lastPos = transform.position;
        Vector3 back = Vector3.Cross(Vector3.up, sphereCenter.position - transform.position);
        while (randRotation > 0 && ((transform.position - lastPos).y >= 0 || transform.position.y >= minY))
        {
            float mov = Time.deltaTime * conf.rotSpeed;
            if (mov > randRotation)
                mov = randRotation;
            randRotation -= mov;
            lastPos = transform.position;
            transform.RotateAround(sphereCenter.position, back, mov * sign);
            yield return null;
        }
        isInMove = false;
    }
    public float minY;
    public IEnumerator GetAboveThePoint(Vector3 point)
    {
        point.y = Mathf.Sqrt(sphereRadius * sphereRadius - point.x * point.x -
        point.z * point.z);
        float dist = (point - transform.position).magnitude;
        Vector3 startPos = transform.position;
        for (float t = 0; t < dist;)
        {
            t += conf.cheatMoveSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, point, t / dist);

            yield return null;
        }
        isInMove = false;
    }
    public static Dictionary<string, float> tagToDamage;
    void Awake()
    {
        singleton = this;
        conf = phases[0];

        Vector3 pos = UnityEngine.Random.onUnitSphere * sphereRadius;
        do
        {
            pos.y = Mathf.Abs(pos.y);
            if (pos.y > minY)
                break;
            pos = UnityEngine.Random.onUnitSphere * sphereRadius;
        } while (true);
        transform.position = pos;
    }
    public GameObject fireZoneParticlesPrefab;
    public float groundLevel = 3.2f;
    bool controlFireZoneFlag = false;
    IEnumerator controlFireZone(GameObject fireZone)
    {
        var ps = fireZone.GetComponentInChildren<ParticleSystem>();
        var psMain = ps.main;
        var vel = ps.velocityOverLifetime;
        var orbZ = vel.orbitalZ;
        orbZ.constant /= conf.fireZoneScale;
        vel.orbitalZ = orbZ;
        for (float t = 0; t < 1;){
            t+= Time.deltaTime*conf.speedOfFireZoneIgnition;
            
            psMain.startColor = Color.Lerp(conf.startFireZone,conf.endFireZone,t);
            yield return null;
        }
        fireZone.GetComponent<SphereCollider>().enabled = true;

        controlFireZoneFlag = false;
        yield return new WaitForSeconds(conf.zoneLifeTime);
        ps.Stop();
        for (float t = 0; t < 1;){
            t+= Time.deltaTime*conf.speedOfFireZoneIgnition;
            
            psMain.startColor = Color.Lerp(conf.endFireZone,conf.startFireZone,t);
            yield return null;
        }
        Destroy(fireZone);
    }
    IEnumerator StrikeFromAbove()
    {
        isInMove = true;
        var move = StartCoroutine(GetAboveThePoint(pointToLookAt.position));
        while (isInMove)
        {
            if (unableToMove)
            {
                StopCoroutine(move);
                isInMove = false;
                isInAttack = false;
                yield break;
            }

            yield return null;
        }
        Vector3 point = transform.position;
        point.y = groundLevel;
        GameObject fireZone = Instantiate(fireZoneParticlesPrefab, point, Quaternion.identity);
        fireZone.transform.localScale = conf.fireZoneScale*Vector3.one;
        controlFireZoneFlag = true;
        StartCoroutine(controlFireZone(fireZone));
        while (controlFireZoneFlag)
        {
            if (unableToMove)
            {
                break;
            }

            yield return null;
        }
        isInAttack = false;
    }
    IEnumerator BigSlowLaser()
    {
        fixGazeOnAttack=true;
        attackSound.clip = raySound1;
        attackSound.loop = true;
        attackSound.Play();
        GameObject attackingRay = Instantiate(bigRayPrefab, rayStandartPoint.position, rayStandartPoint.rotation);
        RaycastHit hit;
        Ray ray = new Ray(attackingRay.transform.position, attackingRay.transform.forward);
        if (Physics.Raycast(ray, out hit, (pointToLookAt.position - transform.position).magnitude, toPlayerMask))
        {
            GameObject effects = Instantiate(rayPremarkPrefab, hit.point, Quaternion.identity);
            StartCoroutine(RayGrow(attackingRay, hit.distance, effects));
        }
        else
        {
            Destroy(attackingRay);
        }

        yield return null;
        while (!unableToMove && unfinishedRays > 0)
        {
            yield return null;
        }
        attackSound.Stop();
        isInAttack = false;
    }
    IEnumerator SunBurn()
    {
        attackSound.clip = slowLaserSound;
        attackSound.loop=true;
        attackSound.Play();
        while (HasEyeContactWithPlayer())
        {
            Player.getSunned();
            yield return null;
        }
        attackSound.Stop();
        isInAttack = false;
    }
    public PhaseInfo conf;
    public GameObject rayPrefab, bigRayPrefab;
    public GameObject rayPremarkPrefab;
    public Transform rayStandartPoint;
    public int unfinishedRays = 0;
    IEnumerator RayGrow(GameObject ray, float endSize, GameObject effects, float speed = -1)
    {
        ray.GetComponentInChildren<MeshRenderer>().material = conf.rayMaterial;
        var directionVectors = new Vector3[] { Vector3.up, Vector3.right, Vector3.back };
        unfinishedRays++;
        float raySpeed = speed == -1 ? UnityEngine.Random.Range(conf.minRaySpeed, conf.maxRaySpeed) : speed;
        Vector3 startRay = new Vector3(1, 1, 0);
        Vector3 endRay = new Vector3(1, 1, endSize / 2);
        for (float t = 0; t < 1;)
        {
            t += Time.deltaTime * raySpeed;
            if (t > 1)
                t = 1;
            ray.transform.localScale = Vector3.LerpUnclamped(startRay, endRay, t);
            yield return null;
        }
        float timeLeft = conf.rayTime;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            foreach (var vec in directionVectors)
                ray.transform.RotateAround(transform.position, vec, UnityEngine.Random.Range(-conf.rayMaxDivergenceDegree, conf.rayMaxDivergenceDegree) * Time.deltaTime * 3f);

            yield return null;
        }
        Destroy(ray);
        Destroy(effects);
        unfinishedRays--;
    }
    public AudioClip raySound1,slowLaserSound,stormSound,audStop;
    IEnumerator ManyRaysAttack()
    {
        fixGazeOnAttack=true;
        int rays = UnityEngine.Random.Range(conf.minRays, conf.maxRays + 1);
        int allRays = rays + UnityEngine.Random.Range(conf.minSecRays, conf.maxSecRays + 1);
        var directionVectors = new Vector3[] { Vector3.up, Vector3.right, Vector3.back };
        float curDivergenceDegree = conf.rayMaxDivergenceDegree;
        attackSound.clip = raySound1;
        attackSound.loop = true;
        attackSound.Play();
        for (int i = 0; i < allRays; ++i)
        {
            if (i == rays)
            {
                curDivergenceDegree = conf.secRayMaxDivergenceDegree;
            }
            GameObject attackingRay = Instantiate(rayPrefab, rayStandartPoint.position, rayStandartPoint.rotation);
            foreach (var vec in directionVectors)
            {
                attackingRay.transform.RotateAround(transform.position, vec, UnityEngine.Random.Range(-curDivergenceDegree, curDivergenceDegree));
            }
            RaycastHit hit;
            Ray ray = new Ray(attackingRay.transform.position, attackingRay.transform.forward);
            if (Physics.Raycast(ray, out hit, (pointToLookAt.position - transform.position).magnitude, toPlayerMask))
            {
                GameObject effects = Instantiate(rayPremarkPrefab, hit.point, Quaternion.identity);
                StartCoroutine(RayGrow(attackingRay, hit.distance, effects));
            }
            else
            {
                Destroy(attackingRay);
            }
            yield return null;
        }
        yield return null;
        while (!unableToMove && unfinishedRays > 0)
        {
            yield return null;
        }
        attackSound.Stop();
        isInAttack = false;
    }
    Func<IEnumerator>[] allAttacks;
    Func<IEnumerator>[] attacks = new Func<IEnumerator>[0];
    public LayerMask toPlayerMask;
    public bool HasEyeContactWithPlayer()
    {

        RaycastHit hit;
        Ray ray = new Ray(transform.position, pointToLookAt.position - transform.position);
        if (Physics.Raycast(ray, out hit, (pointToLookAt.position - transform.position).magnitude, toPlayerMask))
        {
            return false;
        }
        return true;
    }
    bool strikeFromAboveFlag = false;
    IEnumerator MoveUntillSomething(Func<IEnumerator>[] moves, bool stopOnEyeContact, int howManyMoves = -1, float minTime = 0.1f)
    {
        int howManyMovesSave = howManyMoves; float minTimeSave = minTime;
        while (true)
        {
            if (howManyMoves < -10)
            {
                // force above player, beacause they've hidden well.
                strikeFromAboveFlag = true;
                break;
            }
            if (unableToMove || howManyMoves-- == 0)
            {
                if (!unableToMove && minTime > 0)
                {
                    yield return MoveUntillSomething(moves, stopOnEyeContact, howManyMovesSave, minTimeSave);
                }
                break;

            }
            isInMove = true;
            var move = StartCoroutine(moves[UnityEngine.Random.Range(0, moves.Length)].Invoke());
            while (isInMove)
            {
                if (unableToMove || (stopOnEyeContact ? HasEyeContactWithPlayer() : false))
                {
                    if (!unableToMove)
                    {
                        yield return new WaitForSeconds(0.25f);
                    }
                    StopCoroutine(move);
                    isInMove = false;
                    yield break;
                }

                yield return null;
                minTime -= Time.deltaTime;
            }
        }
    }
    IEnumerator SunAI()
    {
        Func<IEnumerator>[] moves = new Func<IEnumerator>[] { RotateHorizontally, RotateVertically };
        while (true)
        {
            while (unableToMove) { yield return null; }
            yield return MoveUntillSomething(moves, stopOnEyeContact: false, howManyMoves: 1);
            yield return MoveUntillSomething(moves, stopOnEyeContact: true, howManyMoves: -1);
            if (unableToMove)
                continue;


            isInAttack = true;
            var move = StartCoroutine(strikeFromAboveFlag ? StrikeFromAbove() : attacks[UnityEngine.Random.Range(0, attacks.Length)].Invoke());
            while (isInAttack)
            {
                if (unableToMove)
                {
                    StopCoroutine(move);
                    isInMove = false;
                    isInAttack = false;
                    attackSound.Stop();
                    break;
                }

                yield return null;
            }
            if (conf.attacksPerPhase-- <=0){
                unableToMove = true;
                LaserTower.Allow();
            }
            //   yield return GetAboveThePoint(sphereCenter.position + new Vector3(10,10,10));
        }
    }
    public bool isInMove = false, isInAttack = false;
    public bool unableToMove = true;
    public static void UpdateDamageDic()
    {
        Color color = singleton.conf.imageColor;
        color.a = 0f;
        Debug.Log(color);
        Debug.Log(PopUp.singleton);
        Debug.Log(PopUp.singleton.blackScreen);
        Debug.Log(PopUp.singleton.blackScreen.color);
        PopUp.singleton.blackScreen.color = color;
        singleton.GetComponentInChildren<MeshRenderer>().material = singleton.conf.sunMaterial;
        tagToDamage = new Dictionary<string, float>();
        tagToDamage.Add("Ray", singleton.conf.rayDamage);
        tagToDamage.Add("BigRay", singleton.conf.bigRayDamage);
        tagToDamage.Add("SunBurn", singleton.conf.burnDamage);
        tagToDamage.Add("Zone", singleton.conf.fireZoneDamage);
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
            singleton.conf = singleton.phases[singleton.curPhase];
            UpdateDamageDic();
            singleton.attacks = new Func<IEnumerator>[singleton.phases[singleton.curPhase].availableAttacks.Length];
            for (int i = 0; i < singleton.attacks.Length; ++i)
            {
                singleton.attacks[i] = singleton.allAttacks[singleton.phases[singleton.curPhase].availableAttacks[i]];
            }
            singleton.StartCoroutine(singleton.nextStageAnimation());
        }
    }
    public float scalingSpeed = 0.5f;
    public AudioSource attackSound;
    IEnumerator nextStageAnimation()
    {
        SkyboxManager.Darken();
        Player.player.Stop();
        Player.player.clip=audStop;
        Player.player.loop=  false;
        Player.player.Play();
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
        Player.player.clip=conf.track;
        Player.player.loop = true;
        Player.player.Play();
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
        curScale = newScale;
        singleton.unableToMove = false;
    }
    // Update is called once per frame
    bool fixGazeOnAttack=false;
    Color prevMain, prevSec;
    float curScale = 1;
    void Update()
    {
        Debug.Log(curPhase);
        if (fixGazeOnAttack){
            fixGazeOnAttack = isInAttack;
        }
        if(!fixGazeOnAttack)
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
