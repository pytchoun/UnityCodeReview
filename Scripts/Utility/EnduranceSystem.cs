using System;
using System.Collections;
using UnityEngine;

public class EnduranceSystem : MonoBehaviour
{
    // Events
    public event EventHandler<OnEnduranceUpdateEventArgs> OnEnduranceUpdate;
    public class OnEnduranceUpdateEventArgs : EventArgs
    {
        public float Endurance;
    }

    [Header("Variables")]
    [Tooltip("The endurance of the character")]
    [SerializeField] private float _endurance = 100f;
    private float _enduranceMax;
    private bool _isSprinting;

    // Variables
    private Coroutine _regeneratingEndurance;
    private Coroutine _consumptionEndurance;

    public float GetEndurance()
    {
        return _endurance;
    }

    private void Awake()
    {
        _enduranceMax = _endurance;
    }

    private void Start()
    {
        if (TryGetComponent(out Player player))
        {
            player.OnSprint += Player_OnSprint;
        }
    }

    private void Player_OnSprint(object sender, Player.OnSprintEventArgs e)
    {
        _isSprinting = e.State;
    }

    private void Update()
    {
        EnduranceCheck();
    }

    private void EnduranceCheck()
    {
        if (_endurance <= 0f)
        {
            _isSprinting = false;
        }

        // If player doesn't sprint and player endurance is not full and regenerating coroutine isn't running
        if (!_isSprinting && _endurance < _enduranceMax && _regeneratingEndurance == null)
        {
            _regeneratingEndurance = StartCoroutine(RegenerateEndurance());
        }
        // If player endurance is full and regenerating coroutine is running or if player sprint and regenerating coroutine is running
        else if ((_endurance >= _enduranceMax && _regeneratingEndurance != null) || (_isSprinting && _regeneratingEndurance != null))
        {
            StopCoroutine(_regeneratingEndurance);
            _regeneratingEndurance = null;
        }

        // If player sprint and consumption coroutine isn't running
        if (_isSprinting && _consumptionEndurance == null)
        {
            _consumptionEndurance = StartCoroutine(EnduranceConsumption());
        }
        // If player doesn't sprint and consumption coroutine is running
        else if (!_isSprinting && _consumptionEndurance != null)
        {
            StopCoroutine(_consumptionEndurance);
            _consumptionEndurance = null;
        }
    }

    private IEnumerator RegenerateEndurance()
    {
        float timeBeforeRegeneration = 3f;
        yield return new WaitForSeconds(timeBeforeRegeneration);
        WaitForSeconds timeToWait = new WaitForSeconds(0.1f);

        while (_endurance < _enduranceMax)
        {
            _endurance += 1f;

            if (_endurance > _enduranceMax)
            {
                _endurance = _enduranceMax;
            }

            OnEnduranceUpdate?.Invoke(this, new OnEnduranceUpdateEventArgs
            {
                Endurance = _endurance
            });

            yield return timeToWait;
        }
        _regeneratingEndurance = null;
    }

    private IEnumerator EnduranceConsumption()
    {
        float timeBeforeConsumption = 0.5f;
        yield return new WaitForSeconds(timeBeforeConsumption);
        WaitForSeconds timeToWait = new WaitForSeconds(0.1f);

        while (_endurance > 0f)
        {
            _endurance -= 1f;

            if (_endurance < 0f)
            {
                _endurance = 0f;
            }

            OnEnduranceUpdate?.Invoke(this, new OnEnduranceUpdateEventArgs
            {
                Endurance = _endurance
            });

            yield return timeToWait;
        }
        _consumptionEndurance = null;
    }
}