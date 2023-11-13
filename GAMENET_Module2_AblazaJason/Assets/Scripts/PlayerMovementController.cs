using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementController : MonoBehaviour
{
    public Joystick _joystick;
    public FixedTouchField FixedTouchField;

    private RigidbodyFirstPersonController _rbFPController;

    private Animator _animator;

    private void Start()
    {
        _rbFPController = this.GetComponent<RigidbodyFirstPersonController>();
        _animator = this.GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (_rbFPController != null)
        {
            _rbFPController._joystickInputAxis.x = _joystick.Horizontal;
            _rbFPController._joystickInputAxis.y = _joystick.Vertical;
            _rbFPController.mouseLook.lookInputAxis = FixedTouchField.TouchDist;

            _animator.SetFloat("Horizontal", _joystick.Horizontal);
            _animator.SetFloat("Vertical", _joystick.Vertical);

            if (Mathf.Abs(_joystick.Horizontal) > 0.9 || Mathf.Abs(_joystick.Vertical) > 0.9)
            {
                _animator.SetBool("IsRunning", true);
                _rbFPController.movementSettings.ForwardSpeed = 10;
            }

            else
            {
                _animator.SetBool("IsRunning", false);
                _rbFPController.movementSettings.ForwardSpeed = 5;
            }
        }
    }
}
