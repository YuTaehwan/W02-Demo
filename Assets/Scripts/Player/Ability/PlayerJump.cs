using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using W02;

public class PlayerJump : PlayerAbility
{
    public float jumpHeight = 15;

    protected override void HandleInput()
    {
        if (_controller.IsOnGround && InputManager.Instance.JumpButton) // �ٴڿ� ���� ���� ���� ����
        {
            _player.playerInfo.isJumping = true;
            _controller.SetYVelocity(jumpHeight);
        }

        if (_player.playerInfo.isJumping && !_controller.WasOnGound && !_controller.IsOnGround)
        {
            _player.playerInfo.isJumping = false;
        }
    }
}
