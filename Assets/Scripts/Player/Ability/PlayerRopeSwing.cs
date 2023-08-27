using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using W02;


public class PlayerRopeSwing : PlayerAbility
{
    public Chain rope;
    public float ropeSpeed = 10f;
    public float ropeGravityAbs = 20f;
    float targetVelocityX = 0;
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
                UpdatePendulumAcceleration();
            }
            HoldingRope();

        }

        //���� ����
        if (_player.playerInfo.ropeState == Player.RopeState.HOLDING && !_hookButtonClicked)
        {
            rope.ChainReset();
            _player.playerInfo.ropeState = Player.RopeState.FAILED;
            _player.ChangeState(Player.State.IDLE);

            //�ӵ�����
        }


    }

    private void CreateRope()
    {
        Vector2 playerPosition = (Vector2)_player.transform.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //rope.chainMaxLength = Vector2.Distance(playerPosition, mousePosition);
        float ropeLength = Vector2.Distance(playerPosition, mousePosition);
        _controller.SetVelocity(Vector2.zero);
        rope.CreateChain(ropeLength);
        rope.ChainConnect(playerPosition, mousePosition, ropeLength, 0.5f);
    }

    private void HoldingRope()
    {
        //CalculateVelocity();
        CalculateRopeSwinging();
        //Debug.Log(" targetelocityX: " + targetVelocityX
        //       + " theta: " + theta
        //       + " angularAcceleration: " + angularAcceleration
        //       + " Current velocity: " + _controller.controllerPhysics.velocity);
    }
    public void CalculateRopeSwinging()
    {
        AddVelocity();
        Vector2 playerPosition = _player.transform.position;
        rope.nodes[0].position = playerPosition;
        return;
        //Vector2 velocity = new Vector2(targetVelocityX, 0) + (Vector2)_controller.controllerPhysics.velocity;

        Chain.ChainNode lastRopeNode = rope.nodes[rope.chainMaxCount - 1];
        Vector2 ropeDirection = (Vector2)lastRopeNode.position - playerPosition;
        Vector2 v1 = TranslateForce(new Vector2(targetVelocityX, 0), ropeDirection, RoundNormalize(targetVelocityX));
        Vector2 v3 = (v1);

        Vector2 nextMovePoint = (Vector2)playerPosition + v3 * 0.01f;
        Vector2 nextMovePointToOriginChainNode = rope.chainMaxLength * (lastRopeNode.position - (Vector2)nextMovePoint).normalized;
        Vector2 newMoveVector = -nextMovePointToOriginChainNode + ropeDirection;
        this._controller.AddVelocity(newMoveVector);

        //if (Vector2.Distance((Vector2)playerPosition + newMoveVector * Time.deltaTime, lastRopeNode.position) > rope.chainMaxLength)
        //{
        //    //float deg = Vector2.Angle(velocity, playerMoveVector);
        //    //if (deg > 90.0f)
        //    //{
        //    //    deg = 89.99f;
        //    //}
        //}
        //else
        //{

        //    this._controller.AddVelocity(newMoveVector);
        //}

    }

    Vector2 TranslateForce(Vector2 rawForce, Vector2 ropeDirection, float dir)
    {
        Vector2 translatedVector;

        Chain.ChainNode lastRopeNode = rope.nodes[rope.chainMaxCount - 1];
        Vector2 playerPosition = _player.transform.position;
        Vector3 toOriginChainNode = (ropeDirection);
        //Debug.DrawRay(playerPosition, toOriginChainNode, Color.red);
        Vector3 playerMoveVector = Vector3.Cross(new Vector3(0, 0, -1), toOriginChainNode);


        if (dir < 0)
            playerMoveVector = -1 * playerMoveVector;
        //Vector3 nextMovePoint = (Vector3)playerPosition + playerMoveVector * direction * 0.01f;
        //Vector2 nextMovePointToOriginChainNode = rope.chainMaxLength * (lastRopeNode.position - (Vector2)nextMovePoint).normalized;
        //playerMoveVector = -nextMovePointToOriginChainNode + (Vector2)toOriginChainNode;

        float deg = Vector2.Angle(rawForce, playerMoveVector);
        translatedVector = ((Vector2)playerMoveVector).normalized * rawForce.magnitude * Mathf.Cos(Mathf.Deg2Rad * deg);

        return translatedVector;
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
        int currentXDirection = RoundNormalize(targetVelocityX);
        float targetMaxSpeed = maxMoveSpeed * penaltySpeedRope;
        bool isFasterThanMaxSpeed = Mathf.Abs(targetVelocityX) > targetMaxSpeed;
        float decelerationWhenMaxSpeed = _controller.IsOnGround ? decelerationMaxSpeedOnGround : decelerationMaxSpeedOnAir;
        float deceleration = _controller.IsOnGround ? decelerationOnGround : decelerationOnAir;
        float acceleration = _controller.IsOnGround ? accelerationOnGround : accelerationOnAir;
   
        if (currentXDirection == 1) // ����������� ->
        {
            if (xInputDirection == 1) // �Է¹��� ->
            {
                if (!isFasterThanMaxSpeed)
                {
                    targetVelocityX += acceleration * Time.deltaTime;

                    if (targetVelocityX > targetMaxSpeed)
                    {
                        targetVelocityX = targetMaxSpeed;
                    }
                }
            }
            else if (xInputDirection == -1) // �Է¹��� <-
            {
                targetVelocityX -= acceleration * Time.deltaTime;
            }
            else // �Է� 0
            {
                targetVelocityX -= deceleration * Time.deltaTime;
                if (targetVelocityX < 0f)
                {
                    targetVelocityX = 0f;
                }
            }
            if (isFasterThanMaxSpeed)
            {
                targetVelocityX -= decelerationWhenMaxSpeed * Time.deltaTime;
            }
        }
        else if (currentXDirection == -1) // ����������� <-
        {
            if (xInputDirection == -1) // �Է¹��� <-
            {
                if (!isFasterThanMaxSpeed)
                {
                    targetVelocityX -= acceleration * Time.deltaTime;
                    if (targetVelocityX < -targetMaxSpeed)
                    {
                        targetVelocityX = -targetMaxSpeed;
                    }
                }
            }
            else if (xInputDirection == 1) // �Է¹��� ->
            {
                targetVelocityX += acceleration * Time.deltaTime;
            }
            else // �Է� 0
            {
                targetVelocityX += deceleration * Time.deltaTime;
                if (targetVelocityX > 0f)
                {
                    targetVelocityX = 0f;
                }
            }
            if (isFasterThanMaxSpeed)
            {
                targetVelocityX += decelerationWhenMaxSpeed * Time.deltaTime;
            }
        }
        else if (xInputDirection == -1)
        {
            targetVelocityX -= acceleration * Time.deltaTime;
            if (targetVelocityX < -targetMaxSpeed)
            {
                targetVelocityX = -targetMaxSpeed;
            }
        }
        else if (xInputDirection == 1)
        {
            targetVelocityX += acceleration * Time.deltaTime;
            if (targetVelocityX > targetMaxSpeed)
            {
                targetVelocityX = targetMaxSpeed;
            }
        }
    }


    float theta; // �ʱⰢ

    void UpdateTheta(Vector3 hookedPosition, Vector3 currentPosition)
    {
        float Xdistance = hookedPosition.x - currentPosition.x;
        float Ydistance = hookedPosition.y - currentPosition.y;

        theta = Mathf.Atan2(Xdistance, Ydistance);
    }

    float angularAcceleration;
    float centripetalAcceleration;

    void UpdatePendulumAcceleration()
    {
        angularAcceleration = -ropeGravityAbs * Mathf.Sin(theta);
        centripetalAcceleration = Mathf.Pow(_controller.controllerPhysics.velocity.magnitude, 2) / rope.chainMaxLength;
    }

    float rotateSpeed = 0.01f;

    private void AddVelocity()
    {
        Chain.ChainNode lastRopeNode = rope.nodes[rope.chainMaxCount - 1];
        Vector2 playerPosition = _player.transform.position;
        Vector2 toOriginChainNode = (Vector2)lastRopeNode.position - playerPosition;

        Vector2 t = (toOriginChainNode).normalized * centripetalAcceleration;
        Vector2 g = ((Vector2)(Vector3.Cross(new Vector3(0, 0, 1), toOriginChainNode))).normalized * angularAcceleration ;

        Debug.Log(t + " , " + g + " t + g : " + (t + g));
        g = TranslateForce(g, toOriginChainNode, RoundNormalize(g.x));
        _controller.AddVelocity((t + g) * Time.deltaTime);
    }

}
