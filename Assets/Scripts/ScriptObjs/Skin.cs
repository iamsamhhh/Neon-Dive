using UnityEngine;

[CreateAssetMenu(fileName = "new skin", menuName = "Skin/Create skin")]
public class Skin : ScriptableObject
{
    public string id;
    public int cost;
    public bool haveTrail;
    public Color trailColor;
    public Sprite sprite;
    public Color spriteColor;
    public Texture2D thumbnail;
}