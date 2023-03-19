using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Tooltip("The announcement text")]
    [SerializeField] private Text _announcementText;
    [Tooltip("The number of zombie wave")]
    [SerializeField] private Text _zombieWaveNumberText;
    [Tooltip("The number of zombie remaining")]
    [SerializeField] private Text _zombieRemainingText;
    [Tooltip("The health of the player")]
    [SerializeField] private Text _playerHealthText;
    [Tooltip("The endurance of the player")]
    [SerializeField] private Text _playerEnduranceText;
    [Tooltip("The ammo of the equipped weapon")]
    [SerializeField] private Text _playerWeaponAmmoText;

    public static PlayerUI Instance { get; private set; }

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

    public void SetPlayerHealthText(float health)
    {
        _playerHealthText.text = health.ToString();
    }

    public void SetPlayerEnduranceText(float endurance)
    {
        _playerEnduranceText.text = endurance.ToString();
    }

    public void SetPlayerWeaponAmmoText(int currentAmmo, int maxAmmo)
    {
        _playerWeaponAmmoText.text = currentAmmo.ToString() + "/" + maxAmmo.ToString();
    }

    public void SetZombieWaveNumberText(int currentWave, int maxWave)
    {
        _zombieWaveNumberText.text = currentWave.ToString() + "/" + maxWave.ToString();
    }

    public void SetZombieRemainingText(int zombieNumber)
    {
        _zombieRemainingText.text = zombieNumber.ToString();
    }

    public void SetAnnouncementText(string text)
    {
        _announcementText.text = text;
    }

    public IEnumerator EnableAnnouncement(bool state, string message = null)
    {
        if (state)
        {
            if (message != null)
            {
                SetAnnouncementText(message);
            }
            _announcementText.gameObject.SetActive(state);
            yield return new WaitForSeconds(2f);
            _announcementText.gameObject.SetActive(!state);
        }
        else
        {
            _announcementText.gameObject.SetActive(state);
        }   
    }
}