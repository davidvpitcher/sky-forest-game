using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientItem", menuName = "Inventory/ClientItem", order = 2)]
public class ItemGuideClient : ItemGuideBase
{
    public GameObject prefabWhenDropped;
    public GameObject menuprefab;
    public Sprite itemSprite;
    public GameObject Stage1Prefab;
    public GameObject Stage2Prefab;
    public GameObject Stage3Prefab;
    public GameObject Stage4Prefab;
    public GameObject Stage5Prefab;
    public GameObject Stage6Prefab;
}
