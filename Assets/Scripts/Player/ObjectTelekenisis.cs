using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectTelekenis : MonoBehaviour
{
    [Header("Settings")]
    public float grabRange = 10f;
    public float holdDistance = 3f;
    public float moveSpeed = 10f;
    public float throwForce = 10f;
    public LayerMask mainWeapon;

    [Header("References")]
    public GameObject UICrosshair;
    public GameObject UIPickUp;
    public Transform cameraTransform;

    private Rigidbody grabbedRigidbody;
    private ThrowableObject throwableObject;
    private Quaternion originalRotation;

    void Update()
    {
        // Stops all telekinesis logic while paused
        if (GameManager.PauseGame)
        {
            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.useGravity = true;
                grabbedRigidbody = null;
                throwableObject = null;
            }

            UICrosshair.SetActive(true);
            UIPickUp.SetActive(false);
            return;
        }

        // If not holding anything
        if (grabbedRigidbody == null)
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, grabRange, mainWeapon))
            {
                if (hit.rigidbody != null)
                {
                    UICrosshair.SetActive(false);
                    UIPickUp.SetActive(true);

                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        grabbedRigidbody = hit.rigidbody;
                        grabbedRigidbody.useGravity = false;
                        grabbedRigidbody.linearVelocity = Vector3.zero;
                        grabbedRigidbody.angularVelocity = Vector3.zero;
                        originalRotation = grabbedRigidbody.rotation;

                        throwableObject = grabbedRigidbody.GetComponent<ThrowableObject>();
                        if (throwableObject != null)
                            throwableObject.isThrown = false;

                        UICrosshair.SetActive(true);
                        UIPickUp.SetActive(false);
                    }
                }
            }
            else
            {
                UICrosshair.SetActive(true);
                UIPickUp.SetActive(false);
            }

            return;
        }

        // Holding an object: move it
        Vector3 targetPos = cameraTransform.position + cameraTransform.forward * holdDistance;
        grabbedRigidbody.linearVelocity = (targetPos - grabbedRigidbody.position) * moveSpeed;
        grabbedRigidbody.rotation = originalRotation;

        // Throw (right-click)
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            grabbedRigidbody.useGravity = true;
            grabbedRigidbody.AddForce(cameraTransform.forward * throwForce, ForceMode.VelocityChange);

            if (throwableObject != null)
                throwableObject.isThrown = true;

            grabbedRigidbody = null;
            throwableObject = null;
            return;
        }

        // Drop (release left-click)
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            grabbedRigidbody.useGravity = true;

            if (throwableObject != null)
                throwableObject.isThrown = false;

            grabbedRigidbody = null;
            throwableObject = null;
            return;
        }
    }
}
