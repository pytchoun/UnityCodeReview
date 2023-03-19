using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Tooltip("The menu panel")]
    [SerializeField] private GameObject _menuPanel;

    public static Menu Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleMenu()
    {
        _menuPanel.SetActive(!_menuPanel.activeSelf);
        //if (InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInFixedUpdate)
        //{
        //    InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        //}
        //else
        //{
        //    InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
        //}
        Time.timeScale = Time.timeScale == 1f ? 0f : 1f;
        AudioListener.pause = !AudioListener.pause;
    }

    private void Reset()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        //InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
    }

    public void RestartGame()
    {
        Reset();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}