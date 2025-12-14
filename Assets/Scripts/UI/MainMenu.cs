using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçiþleri için kütüphane

public class MainMenu : MonoBehaviour
{
    // Inspector'dan sürükleyip býrakacaðýmýz paneller
    [Header("Paneller")]
    public GameObject Panel1;   // Ýçinde Start, Options, Quit butonlarý olan panel
    public GameObject Panel2;   // Ses, Grafik ayarlarýnýn olduðu panel
    [SerializeField] private int sceneIndex;

    // 1. OYUNU BAÞLATMA FONKSÝYONU
    public void StartGame()
    {
            SceneManager.LoadScene(sceneIndex);
    }
    // 2. AYARLARI AÇMA FONKSÝYONU
    public void OpenOptions()
    {
        // Ana menüyü gizle, Ayarlar panelini görünür yap
        Panel1.SetActive(false);
        Panel2.SetActive(true);
    }

    // 3. AYARLARDAN GERÝ DÖNME FONKSÝYONU (Ayarlar paneline bir 'Geri' butonu koyacaðýz)
    public void CloseOptions()
    {
        // Ayarlarý gizle, Ana menüyü tekrar aç
        Panel2.SetActive(false);
        Panel1.SetActive(true);
    }

    // 4. OYUNDAN ÇIKIÞ FONKSÝYONU
    public void QuitGame()
    {
        Debug.Log("Oyundan Çýkýldý!"); // Sadece editörde çalýþtýðýný görmek için
        Application.Quit();
    }
}