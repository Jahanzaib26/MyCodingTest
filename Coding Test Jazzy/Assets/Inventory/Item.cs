using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName ="Scriptable Object/Item")]
public class Item : ScriptableObject
{

    [Header("Only GamePlay")]
    public TileBase tile;

    [Header("Economy")]
    public int price;



    public ItemType type;
    public ActionType actionType;

    public Vector2Int range = new Vector2Int(5, 4);
    [Header("Only UI")]
    public bool stackable=true;
    [Header("both")]
    public Sprite image;

    public GameObject itemPrefab;

}


public enum ItemType
{
    BuildingBlock,Tool
}
public enum ActionType
{
    dig,mine
}