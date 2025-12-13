using UnityEngine;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Wave Configuration", order = 1)]
public class WaveConfig : ScriptableObject
{
    [Header("Level 1 Spawn Amounts")]
    [SerializeField][Min(0)] public int giantAmount;
    [SerializeField][Min(0)] public int vikingAmount;
    [SerializeField][Min(0)] public int scoutAmount;
    [SerializeField][Min(0)] public int wizardAmount;
    [SerializeField][Min(0)] public int archerAmount;

    [Header("Level 2 Spawn Amounts")]
    [SerializeField][Min(0)] public int giantAmountLvl2;
    [SerializeField][Min(0)] public int vikingAmountLvl2;
    [SerializeField][Min(0)] public int scoutAmountLvl2;
    [SerializeField][Min(0)] public int wizardAmountLvl2;
    [SerializeField][Min(0)] public int archerAmountLvl2;

    [Header("Placement Limits")]
    [SerializeField][Min(0)] public int maxPlaceableUnits;
}
