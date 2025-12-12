using UnityEngine;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Wave Configuration", order = 1)]
public class WaveConfig : ScriptableObject
{
    [Header("Spawn Amounts")]
    [SerializeField][Min(0)] public int giantAmount;
    [SerializeField][Min(0)] public int vikingAmount;
    [SerializeField][Min(0)] public int scoutAmount;
    [SerializeField][Min(0)] public int wizardAmount;
    [SerializeField][Min(0)] public int archerAmount;
}