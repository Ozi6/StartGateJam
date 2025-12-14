using UnityEngine;

public class EndScreenManager : MonoBehaviour
{
    public GameObject winScreen;
    public GameObject loseScreen;

    void Start()
    {
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        if (GameResultHolder.Result == GameResult.Win)
        {
            winScreen.SetActive(true);
        }
        else
        {
            loseScreen.SetActive(true);
        }
    }
}
