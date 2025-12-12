using UnityEngine;

public class Player : MonoBehaviour
{
    public int currentLevel;

    public int goldCount;

    public float augmentBar;

    public void changeAugmentBar(float xpGained)
    {
        augmentBar += xpGained;
        //if(augmentBar >= threshold) {
        //    augmentBar -= threshold;
        //      OpenAugmentSelectionModal();
        //}
        return;
    }
}
