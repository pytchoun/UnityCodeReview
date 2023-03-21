using UnityEngine;
using UnityEngine.InputSystem;

public class Inputs : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public bool jump;
	public bool sprint;
	public bool lockCamera = true;
	public float zoom;
	public bool aim;
	public bool shoot;
	public bool reload;
	public bool openMenu;

	[Header("Movement Settings")]
	public bool analogMovement;

	private void OnMove(InputValue value)
	{
        move = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
	{
        look = value.Get<Vector2>();
    }

    private void OnJump(InputValue value)
	{
        jump = value.isPressed;
    }

    private void OnSprint(InputValue value)
	{
        sprint = value.isPressed;
    }

    private void OnLockCamera(InputValue value)
	{
        lockCamera = !value.isPressed;
    }

    private void OnZoom(InputValue value)
	{
        zoom = value.Get<float>();
    }

    private void OnAim(InputValue value)
	{
        aim = value.isPressed;
    }

    private void OnShoot(InputValue value)
	{
        shoot = value.isPressed;
    }

    private void OnReload(InputValue value)
	{
        reload = value.isPressed;
    }

    private void OnOpenMenu(InputValue value)
	{
        openMenu = value.isPressed;
    }
}