using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform Target;

    Vector3 startDistance, moveVec;

    void Start()
    {
        startDistance = transform.position - Target.position;
    }


    void Update()
    {
        moveVec = Target.position + startDistance;
        moveVec.z = 0;
        moveVec.y = startDistance.y;

        transform.position = moveVec;

    }
}
