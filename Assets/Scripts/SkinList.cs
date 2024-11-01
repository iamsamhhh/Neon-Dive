using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skin list", menuName = "Skin/Create skin list")]
public class SkinList : ScriptableObject
{
    public List<Skin> skinList = new List<Skin>();
}
