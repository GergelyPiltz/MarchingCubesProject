using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //public TMP_Text speedDisplay;
    //public TMP_Text stateDisplay;  
    //public TMP_Text slopeDisplay;

    //[Header("Grappling")]
    //public Transform cam;
    //public Transform gunTip;
    //public LayerMask grappable;

    //public float maxGrappleDistance;
    //public float grappleDelayTime;
    //public float overshootYAxis;

    //private Vector3 grapplePoint;

    //public float grapplingCooldown;
    //private float grapplingCooldownTimer;

    //public bool grappling;
    //public bool activeGrapple;
    //public bool enableSpeedLimiting;

    //public LineRenderer lr;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode grappleKey = KeyCode.Mouse1;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool isGrounded;

    [Header("Slope Check")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public enum MovementState
    {
        walk,
        sprint,
        crouch,
        air
    }

    public MovementState state;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        startYScale = transform.localScale.y;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
        //speedDisplay.SetText(rb.velocity.magnitude.ToString());
        //ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.4f, whatIsGround);

        PlayerInput();
        LimitSpeed();
        StateHandler();

        //apply drag
        if (isGrounded/* && !activeGrapple*/)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        //if (grapplingCooldownTimer > 0)
        //{
        //    grapplingCooldownTimer -= Time.deltaTime;
        //}

    }

    //private void LateUpdate()
    //{
    //    if (grappling)
    //    {
    //        lr.SetPosition(0, gunTip.position);
    //    }
    //}

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //jumping
        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {

            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        //grappling
        //if (Input.GetKeyDown(grappleKey))
        //{
        //    StartGrapple();
        //}

    }

    private void StateHandler()
    {
        if (isGrounded && Input.GetKey(sprintKey))
        { //sprinting
            //Debug.Log("Sprint");
            state = MovementState.sprint;
            moveSpeed = sprintSpeed;
        }
        else if (isGrounded)
        { //walking
            //Debug.Log("Walk");
            state = MovementState.walk;
            moveSpeed = walkSpeed;
        }
        else
        { //in the air
            //Debug.Log("Air");
            state = MovementState.air;
        }

        if (Input.GetKey(crouchKey))
        {
            //Debug.Log("Crouch");
            state = MovementState.crouch;
            moveSpeed = crouchSpeed;
        }
    }

    private void MovePlayer()
    {
        //if (activeGrapple) return;
        //movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(10f * moveSpeed * GetSlopeMoveDirection(), ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 10f, ForceMode.Force);
            }
        }
        //on ground
        else if (isGrounded)
        {
            rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }
        else
        //in air
        {
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
        //if(!OnSlope()) {           
        //Debug.Log("Flat");
        //} else {
        //Debug.Log("Slope");
        //} 
    }

    private void LimitSpeed()
    {
        //if (activeGrapple) return;

        if (OnSlope() && !exitingSlope) //limit speed on slope
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        { //limit speed when on flat
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

    }

    private void Jump()
    {
        exitingSlope = true; // exit the slope when about to jump
        //reset y.vel when initiating jump
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    //private void StartGrapple()
    //{
    //    if (grapplingCooldownTimer > 0) return;

    //    grappling = true;

    //    RaycastHit grappleHit;
    //    if (Physics.Raycast(cam.position, cam.forward, out grappleHit, maxGrappleDistance, grappable))
    //    {
    //        grapplePoint = grappleHit.point;

    //        Invoke(nameof(ExecuteGrapple), grappleDelayTime);
    //    }
    //    else
    //    {
    //        grapplePoint = cam.position + cam.forward * maxGrappleDistance;
    //        Invoke(nameof(StopGrapple), grappleDelayTime);
    //    }

    //    lr.enabled = true;
    //    lr.SetPosition(1, grapplePoint);
    //}

    //private void ExecuteGrapple()
    //{
    //    Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
    //    float grapplePointRealitveYPos = grapplePoint.y - lowestPoint.y;
    //    float highestPointOnArc = grapplePointRealitveYPos + overshootYAxis;

    //    if (grapplePointRealitveYPos < 0)
    //    {
    //        highestPointOnArc = overshootYAxis;
    //    }

    //    JumpToPosition(grapplePoint, highestPointOnArc);

    //    Invoke(nameof(StopGrapple), 1f);
    //}

    //private void StopGrapple()
    //{
    //    grappling = false;
    //    activeGrapple = false;
    //    grapplingCooldownTimer = grapplingCooldown;

    //    lr.enabled = false;
    //}



    //https://tenor.com/hu/view/calculation-math-hangover-allen-zach-galifianakis-gif-6219070
    //public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    //{
    //    float gravity = Physics.gravity.y;
    //    float displacementY = endPoint.y - startPoint.y;
    //    Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

    //    Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
    //    Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
    //        + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

    //    return velocityXZ + velocityY;
    //}

    //private Vector3 velocityToSet; ///////////////
    //public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    //{
    //    activeGrapple = true;

    //    velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
    //    Invoke(nameof(SetVelocity), 0.1f);
    //}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (!enableSpeedLimiting)
    //    {
    //        enableSpeedLimiting = true;
    //    }
    //}

    //private void SetVelocity()
    //{
    //    enableSpeedLimiting = false;
    //    rb.velocity = velocityToSet;
    //}



}
