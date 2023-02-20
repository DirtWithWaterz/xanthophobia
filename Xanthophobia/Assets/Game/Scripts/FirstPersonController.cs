using System.Collections;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey) && !IsSliding;
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded && !IsSliding;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool willSlideOnSlopes = true;
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] public KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8.0f;
    [SerializeField] private float idleBreatheSpeed = 0.5f;
    [SerializeField] private float currentSpeed = 0.0f;
    [SerializeField] private float timeToSwitchSpeed = 0.3f;
    bool isWalking = false;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 1.75f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new(0, 0.87f, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField, Range(0, 0.1f)] private float _amplitude = 0.015f;
    [SerializeField, Range(0, 30)] private float _frequency = 10.0f;
    private float defaultYPos = 0;
    private float timer;
    private float idleTimer;

    [Header("Zoom Parameters")]
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;

    // SLIDING PARAMETERS

    private Vector3 hitPointNormal;
    private Vector3 ladderHitPointNormal;
    private bool IsClimbingLadder
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit ladderHit, 2.5f) && ladderHit.collider.gameObject.CompareTag("Ladder"))
            {
                ladderHitPointNormal = ladderHit.normal;
                return Vector3.Angle(ladderHitPointNormal, Vector3.down) < characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    private bool IsSliding
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2.5f))
            {
                if (!slopeHit.collider.gameObject.CompareTag("Ladder"))
                {
                    hitPointNormal = slopeHit.normal;
                    return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;
    private float rotationY = 0;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();
            defaultYPos = characterController.height - 0.2f;

            if (canJump)
                HandleJump();

            if (canCrouch)
                HandleCrouch();

            if (canUseHeadbob)
                HandleHeadbob();

            if (canZoom)
                HandleZoom();

            if (canInteract)
            {
                HandleInteractionCheck();
                HandleInteractionInput();
            }

            ApplyFinalMovements();
        }
    }
    private void LateUpdate()
    {
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        if (canUseHeadbob)
        {
            if (Mathf.Abs(moveDirection.x) == 0f || Mathf.Abs(moveDirection.z) == 0f)
                playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, characterController.height - 0.2f, 0.146f);
        }
        else
        {
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, characterController.height - 0.2f, 0.146f);
        }
    }
    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxisRaw("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        rotationY += Input.GetAxisRaw("Mouse X") * lookSpeedX;

    }

    private void HandleJump()
    {
        if (ShouldJump)
            moveDirection.y = jumpForce;
    }

    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;
        StartCoroutine(LerpCamAnim());

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            if (!isCrouching && !IsSprinting && !IsSliding) isWalking = true; else { isWalking = false; }

            timer += Time.deltaTime * currentSpeed;
            playerCamera.transform.localPosition = new Vector3(
                Mathf.Cos(timer * _frequency / 2) * _amplitude * 2,
                defaultYPos + Mathf.Sin(timer * _frequency) * _amplitude,
                playerCamera.transform.localPosition.z);
            playerCamera.transform.localRotation = new Quaternion(
                playerCamera.transform.localRotation.x,
                playerCamera.transform.localRotation.y,
                Mathf.Sin(timer * _frequency / 2) * _amplitude / 3,
                1);
            idleTimer = timer;
        }
        else
        {
            isWalking = false;
            playerCamera.transform.localRotation = new Quaternion(
                playerCamera.transform.localRotation.x,
                playerCamera.transform.localRotation.y,
                Mathf.Sin(timer * _frequency / 2) * _amplitude / 3,
                1);
            playerCamera.transform.localPosition = new Vector3(
                Mathf.Cos(timer * _frequency / 2) * _amplitude * 2,
                defaultYPos + Mathf.Sin(timer * _frequency) * _amplitude,
                playerCamera.transform.localPosition.z);
        }
        if (!IsSprinting && !isWalking)
        {
            StartCoroutine(IdleCamAnim());
        }
    }

    private IEnumerator IdleCamAnim()
    {
        idleTimer += Time.deltaTime * currentSpeed;
        if (isCrouching)
        {
            playerCamera.transform.localPosition += new Vector3(
                Mathf.Cos((idleTimer / 10) * _frequency / 1.3f) * _amplitude,
                Mathf.Sin((idleTimer / 10) * _frequency) * _amplitude * 1.3f,
                0);
        }
        else
        {
            playerCamera.transform.localPosition += new Vector3(
                Mathf.Cos(idleTimer * _frequency / 1.3f) * _amplitude,
                Mathf.Sin(idleTimer * _frequency) * _amplitude * 1.3f,
                0);
        }

        yield return null;
    }

    private IEnumerator LerpCamAnim()
    {
        float timeElapsed2 = 0;
        while (timeElapsed2 < timeToSwitchSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : isWalking ? walkBobSpeed : idleBreatheSpeed), timeElapsed2 / timeToSwitchSpeed);
            timeElapsed2 += Time.deltaTime;
            yield return null;
        }
    }

    private void HandleZoom()
    {
        if (Input.GetKeyDown(zoomKey))
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(true));
        }
        if (Input.GetKeyUp(zoomKey))
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(false));
        }
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

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (characterController.velocity.y < -1 && characterController.isGrounded)
            moveDirection.y = 0;

        if (willSlideOnSlopes && IsSliding)
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;



        if (IsClimbingLadder)
        {
            if (Input.GetKey(KeyCode.W))
            {
                moveDirection += new Vector3(ladderHitPointNormal.x, ladderHitPointNormal.y, ladderHitPointNormal.z) * 10;
            }
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }


    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    private IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;
        while (timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed / timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = targetFOV;
        zoomRoutine = null;
    }
}


