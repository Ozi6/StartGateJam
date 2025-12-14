using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçiþleri için kütüphane

public class MainMenu : MonoBehaviour
{
    // Inspector'dan sürükleyip býrakacaðýmýz paneller
    [Header("Paneller")]
    public GameObject anaMenuPanel;   // Ýçinde Start, Options, Quit butonlarý olan panel
    public GameObject ayarlarPanel;   // Ses, Grafik ayarlarýnýn olduðu panel
    public SceneAsset oyunSahnesiDosyasi;

    [HideInInspector]
    public string saklananSahneIsmi;

    // 1. OYUNU BAÞLATMA FONKSÝYONU
    public void StartGame()
    {
        // Arka planda kaydettiðimiz ismi kullanarak sahneyi açar
        if (!string.IsNullOrEmpty(saklananSahneIsmi))
        {
            SceneManager.LoadScene(saklananSahneIsmi);
        }
        else
        {
            Debug.LogError("HATA: Inspector'da Sahne seçilmemiþ!");
        }
    }
    // 2. AYARLARI AÇMA FONKSÝYONU
    public void OpenOptions()
    {
        // Ana menüyü gizle, Ayarlar panelini görünür yap
        anaMenuPanel.SetActive(false);
        ayarlarPanel.SetActive(true);
    }

    // 3. AYARLARDAN GERÝ DÖNME FONKSÝYONU (Ayarlar paneline bir 'Geri' butonu koyacaðýz)
    public void CloseOptions()
    {
        // Ayarlarý gizle, Ana menüyü tekrar aç
        ayarlarPanel.SetActive(false);
        anaMenuPanel.SetActive(true);
    }

    // 4. OYUNDAN ÇIKIÞ FONKSÝYONU
    public void QuitGame()
    {
        Debug.Log("Oyundan Çýkýldý!"); // Sadece editörde çalýþtýðýný görmek için
        Application.Quit();
    }
    private void OnValidate()
    {
#if UNITY_EDITOR
        // Eðer kutuya bir sahne sürüklediysen
        if (oyunSahnesiDosyasi != null)
        {
            // O sahnenin ismini alýp saklanan deðiþkene yazar
            saklananSahneIsmi = oyunSahnesiDosyasi.name;
        }
#endif
    }
}