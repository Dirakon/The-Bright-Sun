using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhaseInfo
{
    public Color mainColor,secondaryColor, skyboxColor, imageColor = Color.yellow,startFireZone = new Color(1,0,0,0),endFireZone = new Color(1,0,0,1);
    public Material sunMaterial,rayMaterial;
    public float scale,fireZoneScale=1f;
    public int[] availableAttacks;
    public float rotSpeed;
    public float cheatMoveSpeed;
    public float rayMaxDivergenceDegree = 1f;
    public float rayTime = 4f;
    public float secRayMaxDivergenceDegree = 40f;
    public int maxRays = 20, minRays = 2;
    public int maxSecRays = 20, minSecRays = 2;
    public float minRaySpeed = 0.5f, maxRaySpeed = 1.5f;
    public float rayDamage = 0.5f,bigRayDamage=20f,burnDamage=0.5f;
    public float speedOfFireZoneIgnition = 1f,fireZoneDamage=15f;
    public int attacksPerPhase = 10;
    public float zoneLifeTime = 15f;
    public AudioClip track;
}
