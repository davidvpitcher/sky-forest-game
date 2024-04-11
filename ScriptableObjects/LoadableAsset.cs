using UnityEngine;

[CreateAssetMenu(fileName = "LoadableAsset", menuName = "Loadable Assets/Asset", order = 2)]
public class LoadableAsset : ScriptableObject
{
    public string assetTag;
    public Object asset; 
    public bool autoLoad; 

}
