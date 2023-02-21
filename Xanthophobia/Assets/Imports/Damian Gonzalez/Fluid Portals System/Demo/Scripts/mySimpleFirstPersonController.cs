using UnityEngine;
using System.Collections;

//simple first person controller using rigidbody, by Damián González, specially for portals asset.
public class mySimpleFirstPersonController : MonoBehaviour
{
    private bool ShouldCrouch => Input.GetKeyDown(KeyCode.LeftControl) && !duringCrouchAnimation && Physics.CheckSphere(transform.position - new Vector3(0, 1.5f, 0), .5f);

    Rigidbody rb;
    Transform cam;
    public float walkSpeed = 5f;
    public float runSpeed  = 15f;
    public Vector2 mouseSensitivity = new Vector2(1f, 1f);
    float rotX = 0; //start looking forward
    public float maxVelY = 10f;
    public float jumpImpulse = 10f;

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;
    private KeyCode interactKey = KeyCode.E;
    [SerializeField] private Camera playerCamera;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.6f;
    [SerializeField] private float standingHeight = 1.2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new(0, 0.3f, 0);
    [SerializeField] private Vector3 standingCenter = new(0, 0.6f, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    void Start() {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    void FixedUpdate() {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 forwardNotTilted = new Vector3(transform.forward.x, 0, transform.forward.z);

        rb.velocity = (
            forwardNotTilted * speed * Input.GetAxis("Vertical")    //move forward
            +
            transform.right * speed * Input.GetAxis("Horizontal")   //slide to sides
            +
            new Vector3(0, rb.velocity.y , 0)                       //allow jumping & falling
        );


        //look up and down
        rotX += Input.GetAxis("Mouse Y") * mouseSensitivity.y * -1;
        rotX = Mathf.Clamp(rotX, -60f, 60f); //clamp look 
        cam.localRotation = Quaternion.Euler(rotX, 0, 0);

        
        //player tilted? try to make him stand still
        rb.MoveRotation(Quaternion.Lerp(
            transform.rotation * Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSensitivity.x, 0),
            Quaternion.Euler(0, transform.eulerAngles.y, 0),
            .1f
        ));

    }

    private void Update() {
        if (Input.GetButtonDown("Jump") && Physics.CheckSphere(transform.position - new Vector3(0, 1.5f, 0), .5f))
            rb.AddForce(0, jumpImpulse, 0, ForceMode.Impulse);

        HandleInteractionCheck();
        HandleInteractionInput();
        HandleCrouch();
    }

    private void HandleInteractionCheck()
    {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance) && hit.collider.gameObject.layer == 9)
        {
            if (hit.collider.gameObject.layer == 9 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currentInteractable);

                if (currentInteractable)
                    currentInteractable.OnFocus(this.gameObject);
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus(this.gameObject);
            currentInteractable = null;
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out _, interactionDistance, interactionLayer))
        {
            currentInteractable.OnInteract(this.gameObject);
        }

        if (Input.GetKey(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out _, interactionDistance, interactionLayer))
        {
            currentInteractable.OnHoldInteract(this.gameObject);
        }
        if (Input.GetKeyUp(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out _, interactionDistance, interactionLayer))
        {
            currentInteractable.OnReleaseInteract(this.gameObject);
        }
    }

    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private IEnumerator CrouchStand()
    {
        //if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        //    yield break;

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = transform.localScale.y;

        while (timeElapsed < timeToCrouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch), transform.localScale.z);
            //characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //transform.localScale = new Vector3(0, targetHeight, 0);
        //characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }


}
