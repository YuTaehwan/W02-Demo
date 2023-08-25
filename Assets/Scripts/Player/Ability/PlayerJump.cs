using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using W02;

public class PlayerJump : PlayerAbility
{
    float jumpHeight = 4;

    protected override void HandleInput()
    {
        if (_controller.collisions.below && InputManager.Instance.JumpButton) // �ٴڿ� ���� ���� ���� ����
        {
            Debug.Log(Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(_controller.gravity)));

            _controller.SetYVelocity(Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(_controller.gravity)));
        }
    }
}
