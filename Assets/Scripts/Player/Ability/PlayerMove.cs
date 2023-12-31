using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using JetBrains.Annotations;

namespace W02
{
    [RequireComponent(typeof(Controller2D))]
    public class PlayerMove : PlayerAbility
    {
        public GameObject Look;
        //public GameObject Wing;

        float currentVelocityX = 0;
        float velocityXSmoothing;

        [Header("최대 속력")][SerializeField] float maxMoveSpeed = 6f;

        [Header("공중 가속")][SerializeField] float accelerationOnAir = 0.2f;
        [Header("지상 가속")][SerializeField] float accelerationOnGround = 0.2f;

        [Header("공중 감속")][SerializeField] float decelerationOnAir = 0.2f;
        [Header("지상 감속")][SerializeField] float decelerationOnGround = 0.2f;

        [Header("최대 속도 공중 감속 (합연산)")][SerializeField] float decelerationMaxSpeedOnAir = 0.2f;
        [Header("최대 속도 지상 감속 (합연산)")][SerializeField] float decelerationMaxSpeedOnGround = 0.2f;

        [Header("피격될 때 속도 감소치 (곱연산)")][SerializeField] float penaltySpeedDizzy = 0.8f;
        [Header("걸어다닐때 속도 감소치 (곱연산)")][SerializeField] float penaltySpeedWalk = 0.6f;
        [Header("로프 걸때 속도 감소치 (곱연산)")][SerializeField] float penaltySpeedRope = 0.6f;

        float moveSpeed = 6f;

        private void Start()
        {
            _player.ChangeState(Player.State.WALKING);
        }

        protected override void HandleInput()
        {
            if (_player.playerInfo.state == Player.State.ROPE || _player.playerInfo.state == Player.State.IDLE)
            {
                return;
            }
            currentVelocityX = _controller.controllerPhysics.velocity.x;
            this.CalculateVelocity();
            _controller.SetXVelocity(currentVelocityX);
            //this.MoveMent();
        }

        /// <summary>
        /// Calculate Velocity when Player Moves Left or Right in Idle/Walking State
        /// </summary>
        void CalculateVelocity()
        {
            //if (_player.playerInfo.state == Player.State.WALL_GRAB) return;
            //if (_player.playerInfo.state == Player.State.JUMPING) return;
            // add exception state Up here

            int xInputDirection = RoundNormalize(InputManager.Instance.MoveHorizontal);
            int currentXDirection = RoundNormalize(currentVelocityX);
            float targetMaxSpeed = MaxSpeedSetting(_player.playerInfo.state);
            bool isFasterThanMaxSpeed = Mathf.Abs(currentVelocityX) > targetMaxSpeed;
            float decelerationWhenMaxSpeed = _controller.IsOnGround ? decelerationMaxSpeedOnGround : decelerationMaxSpeedOnAir;
            float deceleration = _controller.IsOnGround ? decelerationOnGround : decelerationOnAir;
            float acceleration = _controller.IsOnGround ? accelerationOnGround : accelerationOnAir;

            if (currentXDirection == 1) // 현재진행방향 ->
            {
                if (xInputDirection == 1) // 입력방향 ->
                {
                    if (!isFasterThanMaxSpeed) 
                    {
                        currentVelocityX += acceleration * Time.deltaTime;

                        if (currentVelocityX > targetMaxSpeed)
                        {
                            currentVelocityX = targetMaxSpeed;
                        }
                    }
                }
                else if (xInputDirection == -1) // 입력방향 <-
                {
                    currentVelocityX -= acceleration * Time.deltaTime;
                }
                else // 입력 0
                {
                    currentVelocityX -= deceleration * Time.deltaTime;
                    if (currentVelocityX < 0f)
                    {
                        currentVelocityX = 0f;
                    }
                }
                if (isFasterThanMaxSpeed)
                {
                    currentVelocityX -= decelerationWhenMaxSpeed * Time.deltaTime;
                }
            }
            else if (currentXDirection == -1) // 현재진행방향 <-
            {
                if (xInputDirection == -1) // 입력방향 <-
                {
                    if (!isFasterThanMaxSpeed)
                    {
                        currentVelocityX -= acceleration * Time.deltaTime;
                        if (currentVelocityX < -targetMaxSpeed)
                        {
                            currentVelocityX = -targetMaxSpeed;
                        }
                    }
                }
                else if (xInputDirection == 1) // 입력방향 ->
                {
                    currentVelocityX += acceleration * Time.deltaTime;
                }
                else // 입력 0
                {
                    currentVelocityX += deceleration * Time.deltaTime;
                    if (currentVelocityX > 0f)
                    {
                        currentVelocityX = 0f;
                    }
                }
                if (isFasterThanMaxSpeed)
                {
                    currentVelocityX += decelerationWhenMaxSpeed * Time.deltaTime;
                }
            }
            else if (xInputDirection == -1)
            {
                currentVelocityX -= acceleration * Time.deltaTime;
                if (currentVelocityX < -targetMaxSpeed)
                {
                    currentVelocityX = -targetMaxSpeed;
                }
            }
            else if (xInputDirection == 1)
            {
                currentVelocityX += acceleration * Time.deltaTime;
                if (currentVelocityX > targetMaxSpeed)
                {
                    currentVelocityX = targetMaxSpeed;
                }
            }

            LookDirection(currentXDirection);
        }


        /// <summary>
        /// Returns targetMaxSpeed based on player's state
        /// </summary>
        /// <param name="_state"></param>
        /// <returns></returns>
        float MaxSpeedSetting(Player.State _state)
        {
            switch (_state)
            {
                case Player.State.WALKING:
                    return maxMoveSpeed * penaltySpeedWalk;
                case Player.State.DIZZY:
                    return maxMoveSpeed * penaltySpeedDizzy;
                case Player.State.ROPE:
                    return maxMoveSpeed * penaltySpeedRope;
                default:
                    return maxMoveSpeed;
            }
        }

        /// <summary>
        /// To make normalized round int Value
        /// </summary>
        /// <param name="_value"></param>
        int RoundNormalize(float _value)
        {
            if (_value == 0f)
            {
                return 0;
            }
            if (_value >= 0f)
            {
                return 1;
            }
            return -1;
        }

        void LookDirection(int _lookDir)
        {
            if(_lookDir == 1)
            {
                Look.transform.localScale = new Vector3(1, 1, 1);
                //Wing.transform.localScale = new Vector3(1, 0.5f, 1);
                //Wing.transform.localRotation = Quaternion.Euler(0f, 0f, -10f);
            }
            else if(_lookDir == -1)
            {
                Look.transform.localScale = new Vector3(-1, 1, 1);
                //Wing.transform.localScale = new Vector3(-1, 0.5f, 1);
                //Wing.transform.localRotation = Quaternion.Euler(0f, 0f, 10f);

            }
        }
    }
}