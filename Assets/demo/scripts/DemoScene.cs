using UnityEngine;
using Snugsound;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class DemoScene : MonoBehaviour
{
    // movement config
    public float gravity = -25f;
    public float runSpeed = 8f;
    public float groundDamping = 20f; // how fast do we change direction? higher means faster
    public float inAirDamping = 5f;
    public float jumpHeight = 3f;
    public bool useFixedUpdate = true;

    public Text debugText;

    [HideInInspector]
    private float normalizedHorizontalSpeed = 0;

    private CharacterController2D controller;
    private Animator animator;
    private RaycastHit2D lastControllerColliderHit;
    private Vector3 velocity;
    private new SpriteRenderer renderer;
    private bool jump = false;
    private bool drop = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController2D>();
        renderer = GetComponent<SpriteRenderer>();

        // listen to some events for illustration purposes
        controller.OnControllerCollidedEvent += onControllerCollider;
        controller.OnTriggerEnterEvent += OnTriggerEnterEvent;
        controller.OnTriggerExitEvent += OnTriggerExitEvent;
    }


    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;

        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
    }


    void OnTriggerEnterEvent(Collider2D col)
    {
        Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
    }


    void OnTriggerExitEvent(Collider2D col)
    {
        Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
    }

    #endregion

    private void UpdateController(float deltaTime)
    {
        bool jumping = false;

        if (controller.IsGrounded)
        {
            velocity.y = 0;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            normalizedHorizontalSpeed = 1;
            if (renderer.flipX)
            {
                renderer.flipX = false;
            }

            if (controller.IsGrounded)
            {
                animator.Play(Animator.StringToHash("Run"));
            }
            else if (controller.collisionState.wasGroundedLastFrame)
            {
                animator.Play(Animator.StringToHash("Fall"));
            }

        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            normalizedHorizontalSpeed = -1;
            if (!renderer.flipX)
            {
                renderer.flipX = true;
            }

            if (controller.IsGrounded)
            {
                animator.Play(Animator.StringToHash("Run"));
            }
            else if (controller.collisionState.wasGroundedLastFrame)
            {
                animator.Play(Animator.StringToHash("Fall"));
            }
        }
        else
        {
            normalizedHorizontalSpeed = 0;

            if (controller.IsGrounded)
            {
                animator.Play(Animator.StringToHash("Idle"));
            }
            else if (controller.collisionState.wasGroundedLastFrame)
            {
                animator.Play(Animator.StringToHash("Fall"));
            }
        }

        // we can only jump whilst grounded
        if (controller.IsGrounded && jump)
        {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            animator.Play(Animator.StringToHash("Jump"));
            jumping = true;

        }

        // apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
        var smoothedMovementFactor = controller.IsGrounded ? groundDamping : inAirDamping; // how fast do we change direction?

        velocity.x = Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * runSpeed, deltaTime * smoothedMovementFactor);

        // apply gravity before moving        
        velocity.y += gravity * deltaTime;

        // if holding down bump up our movement amount and turn off one way platform detection for a frame.
        // this lets us jump down through one way platforms
        if (!jumping && controller.IsGrounded && drop)
        {
            velocity.y *= 3f;
            controller.ignoreOneWayPlatformsThisFrame = true;
        }

        controller.Move(deltaTime, velocity * deltaTime, jumping);

        debugText.text = controller.collisionState.ToString().Replace(", ", "\r\n");
        debugText.text += "\r\nDelta: " + velocity;
        debugText.text += "\r\nVelocity: " + controller.velocity;

        // grab our current _velocity to use as a base for all calculations
        velocity = controller.velocity;

    }

    private void Update()
    {
        jump = Input.GetKeyDown(KeyCode.Space);
        drop = Input.GetKeyDown(KeyCode.DownArrow);

        if (!useFixedUpdate)
        {
            UpdateController(Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            UpdateController(Time.fixedDeltaTime);
        }
    }

}
