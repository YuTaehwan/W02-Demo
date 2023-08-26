using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using W02;


public class PlayerRopeSwing : PlayerAbility
{
    public Chain rope;
    public float ropeSpeed = 10f;
    public float ropeGravity = -5f;
    float currentVelocityX = 0;
    float currentVelocityY = 0;

    [Header("���� �ִ� �ӷ�")] [SerializeField] float maxMoveSpeed = 6f;

    [Header("���� ���� ����")] [SerializeField] float accelerationOnAir = 0.2f;
    [Header("���� ���� ����")] [SerializeField] float accelerationOnGround = 0.2f;

    [Header("���� ���� ����")] [SerializeField] float decelerationOnAir = 0.2f;
    [Header("���� ���� ����")] [SerializeField] float decelerationOnGround = 0.2f;

    [Header("���� �ִ� �ӵ� ���� ���� (�տ���)")] [SerializeField] float decelerationMaxSpeedOnAir = 0.2f;
    [Header("���� �ִ� �ӵ� ���� ���� (�տ���)")] [SerializeField] float decelerationMaxSpeedOnGround = 0.2f;

    [Header("���� �ǰݵ� �� �ӵ� ����ġ (������)")] [SerializeField] float penaltySpeedDizzy = 0.8f;
    [Header("���� �ɾ�ٴҶ� �ӵ� ����ġ (������)")] [SerializeField] float penaltySpeedWalk = 0.6f;
    [Header("���� �ɶ� �ӵ� ����ġ (������)")] [SerializeField] float penaltySpeedRope = 1f;

    protected override void HandleInput()
    {
        //hook��ư Ŭ���� state����
        if (_hookButtonClicked
                && _player.playerInfo.state != Player.State.ROPE)
        {
            _player.playerInfo.ropeState = Player.RopeState.HOOKED;
            _player.ChangeState(Player.State.ROPE);
        }
        //HookedRope
        else if (_hookButtonClicked
                && _player.playerInfo.state == Player.State.ROPE 
                && _player.playerInfo.ropeState == Player.RopeState.HOOKED)
        {
            //�÷��̾��� ��ġ ���� �߰��ϱ�
            CreateRope();
            _player.playerInfo.ropeState = Player.RopeState.HOLDING;
        }

        //���� Ÿ����
        if (_hookButtonClicked && _player.playerInfo.ropeState == Player.RopeState.HOLDING)
        {
            HoldingRope();
        }

        //���� ����
        if (_player.playerInfo.ropeState == Player.RopeState.HOLDING && !_hookButtonClicked)
        {
            rope.ChainReset();
            _player.playerInfo.ropeState = Player.RopeState.FAILED;
            _player.ChangeState(Player.State.IDLE);

            _controller.SetXVelocity(0);
        }
    }

    private void CreateRope()
    {
        Vector2 playerPosition = (Vector2)_player.transform.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //rope.chainMaxLength = Vector2.Distance(playerPosition, mousePosition);
        float ropeLength = Vector2.Distance(playerPosition, mousePosition);
        rope.CreateChain(ropeLength);
        rope.ChainConnect(playerPosition, mousePosition, ropeLength, 0.5f);
    }

    private void HoldingRope()
    {
        //    Chain.ChainNode lastRopeNode = rope.nodes[rope.chainMaxCount - 1];
        //    Vector2 playerPosition = (Vector2)_player.transform.position;
        //    rope.nodes[0].position = this.transform.position;
        //    ////////velocity.y = -1 * speed;
        //    if (Vector2.Distance((Vector2)playerPosition + velocity, lastRopeNode.position) > rope.chainMaxLength)
        //    {
        //        Vector3 toOriginChainNode = ((Vector3)lastRopeNode.position - (Vector3)playerPosition);
        //        Debug.DrawRay(playerPosition, toOriginChainNode, Color.red);
        //        Vector3 playerMoveVector = Vector3.Cross(new Vector3(0, 0, -1), toOriginChainNode);
        //        if (velocity.x < 0)
        //            playerMoveVector = -playerMoveVector;
        //        else if (velocity.x == 0)
        //        {
        //            if (playerPosition.x < lastRopeNode.position.x)
        //            {//������ ����
        //                playerMoveVector = playerMoveVector;

        //            }//���� ����
        //            else
        //                playerMoveVector = -playerMoveVector;

        //        }
        //        Vector3 nextMovePoint = (Vector3)playerPosition + playerMoveVector * ropeSpeed * 0.1f;
        //        Vector2 nextMovePointToOriginChainNode = rope.chainMaxLength * (lastRopeNode.position - (Vector2)nextMovePoint).normalized;
        //        playerMoveVector = -nextMovePointToOriginChainNode + (Vector2)toOriginChainNode;
        //        //������ ����
        //        Debug.DrawRay(playerPosition, playerMoveVector, Color.black);
        //        velocity = playerMoveVector;
        //    }
        CalculateVelocity();
        CalculateRopeSwinging();
    }
    public void CalculateRopeSwinging()
    {
        //Vector2 velocity = this._controller.controllerPhysics.velocity;
        int currentXDirection = RoundNormalize(currentVelocityX);
        currentVelocityY = -20f;
        currentVelocityX = 100f * RoundNormalize(InputManager.Instance.MoveHorizontal); ;
        Debug.Log("x: " + currentVelocityX);
        Vector2 velocity = new Vector2(currentVelocityX, currentVelocityY);
        Chain.ChainNode lastRopeNode = rope.nodes[rope.chainMaxCount - 1];
        Vector2 playerPosition = _player.transform.position;
        rope.nodes[0].position = this.transform.position;

        Vector3 toOriginChainNode = ((Vector3)lastRopeNode.position - (Vector3)playerPosition);
        Debug.DrawRay(playerPosition, toOriginChainNode, Color.red);
        Vector3 playerMoveVector = Vector3.Cross(new Vector3(0, 0, -1), toOriginChainNode);

        float direction;
        if (velocity.x == 0.0f)
        {
            if (playerPosition.x > lastRopeNode.position.x)
            {
                direction = -1;
            }
            else
            {
                direction = 1;
            }
        }
        else
        {
            direction = velocity.x;
        }
        Vector3 nextMovePoint = (Vector3)playerPosition + playerMoveVector * direction * 0.01f;
        Vector2 nextMovePointToOriginChainNode = rope.chainMaxLength * (lastRopeNode.position - (Vector2)nextMovePoint).normalized;
        playerMoveVector = -nextMovePointToOriginChainNode + (Vector2)toOriginChainNode;

        Debug.DrawRay(playerPosition, velocity, Color.yellow);
        if (Vector2.Distance((Vector2)playerPosition + velocity * Time.deltaTime, lastRopeNode.position) > rope.chainMaxLength)
        {
            Debug.Log("angle: " + Vector2.Angle(velocity, playerMoveVector));
            float deg =  Vector2.Angle(velocity, playerMoveVector);
            if (deg > 90.0f)
                deg = 89.99f;
            velocity = ((Vector2)playerMoveVector).normalized * velocity.magnitude * Mathf.Cos(Mathf.Deg2Rad * deg / 2);
            Debug.DrawRay(playerPosition, velocity, Color.green);
        }
        this._controller.SetXVelocity(velocity.x);
        this._controller.SetYVelocity(velocity.y);
    }
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


    void CalculateVelocity()
    {
        //if (_player.playerInfo.state == Player.State.WALL_GRAB) return;
        //if (_player.playerInfo.state == Player.State.JUMPING) return;
        // add exception state Up here

        int xInputDirection = RoundNormalize(InputManager.Instance.MoveHorizontal);
        int currentXDirection = RoundNormalize(currentVelocityX);
        float targetMaxSpeed = maxMoveSpeed * penaltySpeedRope;
        bool isFasterThanMaxSpeed = Mathf.Abs(currentVelocityX) > targetMaxSpeed;
        float decelerationWhenMaxSpeed = _controller.IsOnGround ? decelerationMaxSpeedOnGround : decelerationMaxSpeedOnAir;
        float deceleration = _controller.IsOnGround ? decelerationOnGround : decelerationOnAir;
        float acceleration = _controller.IsOnGround ? accelerationOnGround : accelerationOnAir;

        if (currentXDirection == 1) // ����������� ->
        {
            if (xInputDirection == 1) // �Է¹��� ->
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
            else if (xInputDirection == -1) // �Է¹��� <-
            {
                currentVelocityX -= acceleration * Time.deltaTime;
            }
            else // �Է� 0
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
        else if (currentXDirection == -1) // ����������� <-
        {
            if (xInputDirection == -1) // �Է¹��� <-
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
            else if (xInputDirection == 1) // �Է¹��� ->
            {
                currentVelocityX += acceleration * Time.deltaTime;
            }
            else // �Է� 0
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
    }


}
