using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController cc;
    private Animator animator;

    [SerializeField]
    private float _speed = 5.0f,
                  _gravity = 0.4f,
                  _jumpHeight = 20.0f,
                  _sideMoveDistance = 1.5f,
                  _animationTime = 0.9f;


    private float _yVelocity = 0.0f,
                  _dir = 0.0f,
                  _currentDir = 0.0f,
                  _currentSideMoveDistance = 0.0f,
                  _sideMoveSpeed = 0.0f,
                  _tmpDistance = 0.0f,
                  _pushPower = 2.0f;


    private Vector3 sideMove;

    [SerializeField]
    private bool ingame = true,
                 isInMovement = false,
                 inLeftPosition = false,
                 inRightPosition = false,
                 isChangeLane = false;

    [SerializeField]
    private int currentLanePosition = 1,
                lanesCount = 2;
    private void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }
    private void Update()

    {
        if (ingame)
        {
             Vector3 velocity = new Vector3(0, _yVelocity * Time.deltaTime, 0);

            _dir = Input.GetAxis("Horizontal"); // ���������� ���� � ����������

            // ���� ������ �� �������� ����� ��� ������ � ������� ����������� _dir!=0
            // �� isInMovement = true
            if (isInMovement == false & _dir != 0)
            {
                isInMovement = true; // �������� ��������
                isChangeLane = true; // �������� ����� �����
                _currentDir = (int)Mathf.Sign(_dir); // ������� ����������� �� ��������� ����������
                _currentSideMoveDistance = _sideMoveDistance; // ������� ��������� �������� �� ��������� ����������

                StopMovement(); // ������������� �������� � ����������� � ��������� ���� ����� ��������� � ������ ��� ����� �������

                // �������� �������� ��������
                if (_dir > 0 && !inRightPosition)
                    animator.SetTrigger("Right");
                if (_dir < 0 && !inLeftPosition)
                    animator.SetTrigger("Left");
            }
            
            if (isInMovement == true) // ���� ����� ��������� � ��������
            {
                if (isChangeLane) // ����������� ����������� �� 1 �������
                       LanePosition(); // ������ �������

                BorderChecker(); // ��������� � ����� ������� �����

                if (_currentSideMoveDistance > 0) // ���� ��������� ����������� ������ ������ ����
                {
                    velocity = Move(velocity);
                }
                else // �������� ���������������
                {
                    isInMovement = false;
                }
            }

            if (cc.isGrounded == true) // ���� ������ ����� �� �����
                Jump(); // ��� ������� ������� ����� �������
            else
                _yVelocity -= _gravity; // ���� ������ �� �� �����, ������ ������ ��� ��������� ���������
            
            cc.Move(velocity); // ���������� character controller
        }
    }

    /// <summary>
    /// �������� ������ � �������
    /// </summary>
    /// <param name="velocity"></param>
    /// <returns></returns>
    private Vector3 Move(Vector3 velocity)
    {
        _sideMoveSpeed = _sideMoveDistance / _animationTime; // ��������� �������� �������� � ����������� �� ���������� � �������� ��������
        _tmpDistance = _sideMoveSpeed * Time.deltaTime; // ���������� �� ������� ����� ��������� ����� �� 1 ����
        sideMove = Vector3.right * _currentDir * _tmpDistance; // ������� ����������� �� ��������� � ���������� 
        velocity.x = sideMove.x; // �������� �������� ������ � ��.Move
        _currentSideMoveDistance -= _tmpDistance; // �������� �� ��������� ���������� ��������� ���������� �������� ����� �� 1 ����
        return velocity;
    }

    /// <summary>
    /// ���������� ��������� ������ 0-�����, 1-�� ������, 2-������
    /// </summary>
    private void LanePosition()
    {
        currentLanePosition += (int)Mathf.Sign(_dir);
        currentLanePosition = Mathf.Clamp(currentLanePosition, 0, lanesCount);
        isChangeLane = false;
    }

    /// <summary>
    ///���������� inLeftPosition = true ��� inRightposition = true,
    ///inLeftPosition ��������� �������� �������� �����, 
    ///inRightPosition ��������� �������� �������� ������.
    /// </summary>
    private void BorderChecker()
    {
        // ������ ��������� � ����� �������
        if (currentLanePosition == 0)
            inLeftPosition = true;
        else
            inLeftPosition = false;

        //������ ��������� � ������ �������
        if (currentLanePosition == 2)
            inRightPosition = true;
        else
            inRightPosition = false;
    }

    /// <summary>
    /// ��������� ����������� � ��������� ������, �� ��� ��� ������ � ���������
    /// </summary>
    /// <param name="_currentDir"></param>
    private void StopMovement()
    {
        // �� ��� ��������� ������� ����� �� � ����� �������
        if (inLeftPosition && _currentDir < 0)
        {
            isInMovement = false;
            isChangeLane = false;
        }

        // �� ��� ��������� ������� ����� �� � ������ �������
        if (inRightPosition && _currentDir > 0)
        {
            isInMovement = false;
            isChangeLane = false;
        }
    }

    /// <summary>
    /// ������ ������ �� ������� �������
    /// </summary>
    private void Jump()
    {
        // ���� ������ space, ������ �������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _yVelocity = _jumpHeight;
            animator.SetTrigger("Jump");
        }
    }

    private void OnTriggerEnter(Collider other)     // OnTriggerEnter �������� � ������  ���� ��������� ������� � ������� �����
                                                    // ��������������� ������� ��� �������
    {
        if (other.CompareTag("Danger"))
        {
            animator.SetTrigger("Death");
            ingame = false;
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit) // �������� ������ ����� ����� ��������� ��������, ����� ����� ����� � � ���� 
                                                                    // ��������� ������ OnContorllerColliderHit �� �����������
    {
        if (hit.gameObject.CompareTag("Another"))
            Debug.Log("��������");

        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
        {
            return;
        }

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.velocity = pushDir * _pushPower;
    }

    private void OnCollisionEnter(Collision collision) // �������� ������ ���� �� �������� ���� 
                                                        // �� ������������ Rigidbody (� ������ ������ �� ���������)
    {
            Debug.Log(collision.gameObject.name);
    }
}
