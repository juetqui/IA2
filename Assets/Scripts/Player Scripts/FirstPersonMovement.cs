using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    Rigidbody rigidbody;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Debug.LogError("Rigidbody no encontrado en el jugador");
        }
        else
        {
            // Habilitar detección de colisiones continua para triggers
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            //Debug.Log($"Rigidbody configurado: IsKinematic={rigidbody.isKinematic}, Constraints={rigidbody.constraints}");
        }
    }

    void FixedUpdate()
    {
        if (rigidbody == null) return;

        IsRunning = canRun && Input.GetKey(runningKey);

        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        Vector2 targetVelocity = new Vector2(Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

        rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, rigidbody.velocity.y, targetVelocity.y);
        //Debug.Log($"Movimiento aplicado: Velocity={rigidbody.velocity}, Position={transform.position}");
    }
}