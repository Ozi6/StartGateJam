using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject Panel1;   
    public GameObject Panel2;   
    public GameObject Panel3;
    [SerializeField] private int sceneIndex;

    public void StartGame()
    {
            SceneManager.LoadScene(sceneIndex);
    }
    public void OpenPanel2()
    {
        Panel1.SetActive(false);
        Panel2.SetActive(true);
        Panel3.SetActive(false);
    }
    public void OpenPanel1()
    {
        Panel2.SetActive(false);
        Panel1.SetActive(true);
        Panel3.SetActive(false);
    }
    public void OpenPanel3()
    {
        Panel2.SetActive(false);
        Panel1.SetActive(false);
        Panel3.SetActive(true);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}