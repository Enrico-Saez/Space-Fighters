using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform player; // referencie o player
    public float rotationSpeed = 5f;

    private float yaw = 180f;   // rotação horizontal
    private float pitch = 0f; // rotação vertical

    public float minPitch = -80f; // limite inferior
    public float maxPitch = 80f;  // limite superior

    void Update()
    {
        // Input do mouse
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * rotationSpeed;
        pitch -= mouseY * rotationSpeed; // geralmente invertido

        // Limita a rotação vertical pra não virar a câmera de ponta cabeça
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Mantém o CameraRig na posição do player
        transform.position = player.position;

        // Aplica rotação
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}