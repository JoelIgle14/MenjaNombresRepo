using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject helpPanel;


    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    public Toggle multiplierToggle; 


    private void Start()
    {

        // Cargar el valor guardado (por defecto true)
        bool enabled = PlayerPrefs.GetInt("EnableMultiplierCustomer", 1) == 1;
        multiplierToggle.isOn = enabled;

        // Registrar evento cuando cambia
        multiplierToggle.onValueChanged.AddListener(OnMultiplierToggleChanged);


        // Cargar los valores guardados (si existen)
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        masterSlider.value = master;
        musicSlider.value = music;
        sfxSlider.value = sfx;

        ApplyVolumes(master, music, sfx);

        // Mostrar solo el panel principal al iniciar
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);

    }

    // ================= MENÚ PRINCIPAL =================

    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void QuitGame()
    {
        Debug.Log("Juego cerrado.");
        Application.Quit();
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void OpenHelp()
    {
        helpPanel.SetActive(true);
    }

    public void CloseHelp()
    {
        helpPanel.SetActive(false);
    }

    // ================= SLIDERS DE AUDIO =================

    public void OnMasterVolumeChanged(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f); // evita log(0)
        audioMixer.SetFloat("Master", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("Master", value);
    }


    public void OnMusicVolumeChanged(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }


    private void ApplyVolumes(float master, float music, float sfx)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(master) * 20);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(music) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfx) * 20);
    }
    void OnMultiplierToggleChanged(bool value)
    {
        PlayerPrefs.SetInt("EnableMultiplierCustomer", value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
