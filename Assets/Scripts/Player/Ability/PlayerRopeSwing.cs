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
    float direction;

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
            if (rope.nodes.Count != 0)
            {

                UpdateTheta(rope.nodes[rope.chainMaxCount - 1].position, _player.transform.position);
                TestVelocity();
            }
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
        currentVelocityY += ropeGravity * Time.deltaTime;
        Vector2 velocity = new Vector2(currentVelocityX, currentVelocityY);
        Chain.ChainNode lastRopeNode = rope.nodes[rope.chainMaxCount - 1];
        Vector2 playerPosition = _player.transform.position;
        rope.nodes[0].position = this.transform.position;

        Vector3 toOriginChainNode = ((Vector3)lastRopeNode.position - (Vector3)playerPosition);
        Debug.DrawRay(playerPosition, toOriginChainNode, Color.red);
        Vector3 playerMoveVector = Vector3.Cross(new Vector3(0, 0, -1), toOriginChainNode);

        direction = RoundNormalize(currentVelocityX);
        Vector3 nextMovePoint = (Vector3)playerPosition + playerMoveVector * direction * 0.01f;
        Vector2 nextMovePointToOriginChainNode = rope.chainMaxLength * (lastRopeNode.position - (Vector2)nextMovePoint).normalized;
        playerMoveVector = -nextMovePointToOriginChainNode + (Vector2)toOriginChainNode;

        Debug.DrawRay(playerPosition, velocity, Color.yellow);
        if (Vector2.Distance((Vector2)playerPosition + velocity * Time.deltaTime, lastRopeNode.position) > rope.chainMaxLength)
        {
            float deg =  Vector2.Angle(velocity, playerMoveVector);
            if (deg <= 0.1f)
                currentVelocityY = 0;
            if (deg > 90.0f)
            {
                deg = 89.99f;
            }
            velocity = ((Vector2)playerMoveVector).normalized * velocity.magnitude * Mathf.Cos(Mathf.Deg2Rad * deg / 2);
            Debug.DrawRay(playerPosition, velocity, Color.green);
        }
        TestMove();
        this._controller.AddXVelocity(velocity.x * Time.deltaTime);
        this._controller.AddYVelocity(velocity.y * Time.deltaTime);
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


    float theta; // �ʱⰢ
    float omega = 0f; // ���ӵ�

    void UpdateTheta(Vector3 hookedPosition, Vector3 currentPosition)
    {
        float Xdistance = hookedPosition.x - currentPosition.x;
        float Ydistance = hookedPosition.y - currentPosition.y;

        theta = Mathf.Atan2(Ydistance, Xdistance) * Mathf.Rad2Deg - 90f;
        //Debug.Log("theta" + theta);
    }

    float angularAcceleration;

    void TestVelocity()
    {
        angularAcceleration = -ropeGravity / rope.chainMaxLength * Mathf.Sin(theta);
        //Debug.Log("angularAcceleration" + angularAcceleration);
    }

    private void TestMove()
    {
        omega += angularAcceleration * Time.deltaTime;
        theta += omega * Time.deltaTime;

        _controller.AddXVelocity(angularAcceleration * Time.deltaTime);
        //Debug.Log("velocity" + _controller.controllerPhysics.velocity);
    }

}
