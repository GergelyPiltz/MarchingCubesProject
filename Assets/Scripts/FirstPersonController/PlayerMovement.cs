using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //public TMP_Text speedDisplay;
    //public TMP_Text stateDisplay;  
    //public TMP_Text slopeDisplay;

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
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

    }

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

}
