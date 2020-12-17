using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInTheScene 
{
    private int buildingIndex;
    private float zSize;
    private Transform building;

    public BuildingInTheScene(int buildingIndex,  float zSize, Transform building)
    {
        this.buildingIndex = buildingIndex;
        this.zSize = zSize;
        this.building = building;
    }

    public int BuildingIndex { get { return buildingIndex; } set { buildingIndex = value; } }
    public float ZSize { get { return zSize; } set { zSize = value; } }
    public Transform Building { get { return building; } set { building = value; } }
}
