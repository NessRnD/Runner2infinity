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

            _dir = Input.GetAxis("Horizontal"); // подключаем ввод с клавиатуры

            // если объект не движется влево или вправо и выбрано направление _dir!=0
            // то isInMovement = true
            if (isInMovement == false & _dir != 0)
            {
                isInMovement = true; // включаем движение
                isChangeLane = true; // изменяем номер линии
                _currentDir = (int)Mathf.Sign(_dir); // передаём направление во временную переменную
                _currentSideMoveDistance = _sideMoveDistance; // передаём дистанцию перехода во временную переменную

                StopMovement(); // останавливаем анимацию и перемещение с платформы если игрок находится в правой или левой позиции

                // включаем анимацию перехода
                if (_dir > 0 && !inRightPosition)
                    animator.SetTrigger("Right");
                if (_dir < 0 && !inLeftPosition)
                    animator.SetTrigger("Left");
            }
            
            if (isInMovement == true) // если игрок находится в движении
            {
                if (isChangeLane) // выполняется единоразово за 1 переход
                       LanePosition(); // меняем позицию

                BorderChecker(); // проверяем в какой позиции игрок

                if (_currentSideMoveDistance > 0) // если дистанция перемещения игрока больше нуля
                {
                    velocity = Move(velocity);
                }
                else // движение останавливается
                {
                    isInMovement = false;
                }
            }

            if (cc.isGrounded == true) // если объект стоит на земле
                Jump(); // при нажатии пробела игрок прыгает
            else
                _yVelocity -= _gravity; // если объект не на земле, объект падает под действием гравитаци
            
            cc.Move(velocity); // управление character controller
        }
    }

    /// <summary>
    /// Движение игрока в стороны
    /// </summary>
    /// <param name="velocity"></param>
    /// <returns></returns>
    private Vector3 Move(Vector3 velocity)
    {
        _sideMoveSpeed = _sideMoveDistance / _animationTime; // вычисляем скорость движения в зависимости от расстояния и скорости анимации
        _tmpDistance = _sideMoveSpeed * Time.deltaTime; // определяем на сколько будет смещаться игрок за 1 кадр
        sideMove = Vector3.right * _currentDir * _tmpDistance; // передаём направление со скоростью в переменную 
        velocity.x = sideMove.x; // передаем движение игроку в сс.Move
        _currentSideMoveDistance -= _tmpDistance; // вычитаем из временной переменной дистанции расстояние смещения игрок за 1 кадр
        return velocity;
    }

    /// <summary>
    /// определяет положение игрока 0-слева, 1-по центру, 2-справа
    /// </summary>
    private void LanePosition()
    {
        currentLanePosition += (int)Mathf.Sign(_dir);
        currentLanePosition = Mathf.Clamp(currentLanePosition, 0, lanesCount);
        isChangeLane = false;
    }

    /// <summary>
    ///возвращает inLeftPosition = true или inRightposition = true,
    ///inLeftPosition отключает анимацию перехода влево, 
    ///inRightPosition отключает анимацию перехода вправо.
    /// </summary>
    private void BorderChecker()
    {
        // объект находится в левой позиции
        if (currentLanePosition == 0)
            inLeftPosition = true;
        else
            inLeftPosition = false;

        //объект находится в правой позиции
        if (currentLanePosition == 2)
            inRightPosition = true;
        else
            inRightPosition = false;
    }

    /// <summary>
    /// проверяет направление и положение игрока, не даёт ему упасть с платформы
    /// </summary>
    /// <param name="_currentDir"></param>
    private void StopMovement()
    {
        // не даём двигаться объекту когда он в левой позиции
        if (inLeftPosition && _currentDir < 0)
        {
            isInMovement = false;
            isChangeLane = false;
        }

        // не даём двигаться объекту когда он в правой позиции
        if (inRightPosition && _currentDir > 0)
        {
            isInMovement = false;
            isChangeLane = false;
        }
    }

    /// <summary>
    /// Прыжок игрока по нажатию пробела
    /// </summary>
    private void Jump()
    {
        // если нажать space, объект прыгает
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _yVelocity = _jumpHeight;
            animator.SetTrigger("Jump");
        }
    }

    private void OnTriggerEnter(Collider other)     // OnTriggerEnter работает в случае  если коллайдер объекта с которым игрок
                                                    // взаимодействует помечен как триггер
    {
        if (other.CompareTag("Danger"))
        {
            animator.SetTrigger("Death");
            ingame = false;
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit) // работает только когда игрок совершает движение, когда игрок стоит и в него 
                                                                    // врезается объект OnContorllerColliderHit не срабатывает
    {
        if (hit.gameObject.CompareTag("Another"))
            Debug.Log("работает");

        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
        {
            return;
        }

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.velocity = pushDir * _pushPower;
    }

    private void OnCollisionEnter(Collision collision) // работает только если на объектах есть 
                                                        // не кинематичный Rigidbody (в данном случае не сработает)
    {
            Debug.Log(collision.gameObject.name);
    }
}
