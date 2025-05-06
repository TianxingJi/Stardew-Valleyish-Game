using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData_SO", menuName = "Map/MapData")]

public class MapData_SO : ScriptableObject
{
    [Header("Map Information")]
    public int gridWidth;
    public int gridHeight;

    [Header("The original point (The Leftmost button points)")]
    public int originX;
    public int originY;

    [SceneName] public string sceneName;
    public List<TileProperty> tileProperties;
}