using System.Collections.Generic;
using UnityEngine;

public class SimpleSampleCharacterControl : MonoBehaviour
{
    private enum ControlMode
    {
        /// <summary>
        /// Up moves the character forward, left and right turn the character gradually and down moves the character backwards
        /// </summary>
        Tank,
        /// <summary>
        /// Character freely moves in the chosen direction from the perspective of the camera
        /// </summary>
        Direct
    }
    [SerializeField] private Vector3 BeginPos;
    [SerializeField] private float BeginRotate;
    [SerializeField] private float m_moveSpeed = 2;
    [SerializeField] private float m_jumpForce = 4;
    [SerializeField] private float Roadwidth;
    [SerializeField] private float fMovewidth = 0.1f;
    [SerializeField] private float fRotateAngle = 3;

    [SerializeField] private Animator m_animator = null;
    [SerializeField] private Rigidbody m_rigidBody = null;

    [SerializeField] private ControlMode m_controlMode = ControlMode.Tank;

    private float m_currentV = 0;
    private float m_currentH = 0;

    //stop or run
    private float m_force = 1;

    //rotate
    private bool m_right = false;
    private bool m_left = false;

    //horizontal move
    private bool m_rightmove = false;
    private bool m_leftmove = false;

    private Vector3 move;
    //Difference
    private float DifAngle = 0;
    private float DifWidth = 0;

    //private readonly float m_interpolation = 10;
    //private readonly float m_walkScale = 0.33f;


    private bool m_wasGrounded;
    //private Vector3 m_currentDirection = Vector3.zero;

    //jump
    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;
    private bool m_jumpInput = false;

    //
    private float moveDifference = 0;
    private float fix = 0;
    private bool m_move = false;
    //Direction State
    private int dir = 4;
    //x,z
    private bool m_isGrounded;

    private List<Collider> m_collisions = new List<Collider>();

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, BeginRotate, 0);
        dir += (int)(BeginRotate / 90);
        dir %= 4;
        transform.position = BeginPos;
    }
    private void Awake()
    {
        if (!m_animator) { gameObject.GetComponent<Animator>(); }
        if (!m_rigidBody) { gameObject.GetComponent<Animator>(); }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider))
                {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if (validSurfaceNormal)
        {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        }
        else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0) { m_isGrounded = false; }
    }

    private void Update()
    {
        /*if (!m_jumpInput && Input.GetKey(KeyCode.Space))
        {
            m_jumpInput = true;
        }*/
        if (!m_jumpInput && Input.GetAxis("Vertical") > 0)
        {
            m_jumpInput = true;
        }
        if (!m_right && !m_left && Input.GetKeyDown(KeyCode.D))
        {
            m_right = true;
            m_move = true;
            this.Difference();
        }
        if (!m_right && !m_left && Input.GetKeyDown(KeyCode.A))
        {
            m_left = true;
            m_move = true;
            this.Difference();
        }
        if (!m_rightmove && !m_leftmove && Input.GetKeyDown(KeyCode.E))
        {
            m_rightmove = true;
        }
        if (!m_rightmove && !m_leftmove && Input.GetKeyDown(KeyCode.Q))
        {
            m_leftmove = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_force = (m_force == 0) ? 1 : 0;
        }
    }

    private void FixedUpdate()
    {
        m_animator.SetBool("Grounded", m_isGrounded);

        switch (m_controlMode)
        {
            case ControlMode.Direct:
                DirectUpdate();
                break;

            case ControlMode.Tank:
                TankUpdate();
                break;

            default:
                Debug.LogError("Unsupported state");
                break;
        }

        m_wasGrounded = m_isGrounded;
        m_jumpInput = false;
    }

    private void TankUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        if (!m_right && !m_left)
        {
            transform.position += m_moveSpeed * Time.deltaTime * m_force * transform.forward;
            if (m_rightmove)
            {
                this.Horizontalmove(1);
            }
            else if (m_leftmove)
            {
                this.Horizontalmove(-1);
            }
        }
        else if (m_right)
        {

            this.Rotate(1);
            if (m_move)
            {
                this.MoveDifference();
            }
        }
        else if (m_left)
        {

            this.Rotate(-1);
            if (m_move)
            {
                this.MoveDifference();
            }
        }
        m_animator.SetFloat("MoveSpeed", m_force);

        JumpingAndLanding();
    }

    private void DirectUpdate()
    {
        /*float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_walkScale;
            h *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = transform.forward * m_currentV + transform.right * m_currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.rotation = Quaternion.LookRotation(m_currentDirection);
            transform.position += m_moveSpeed * Time.deltaTime * m_currentDirection;

            m_animator.SetFloat("MoveSpeed", direction.magnitude);
        }

        JumpingAndLanding();*/
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && m_jumpInput)
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded)
        {
            m_animator.SetTrigger("Land");
        }

        if (!m_isGrounded && m_wasGrounded)
        {
            m_animator.SetTrigger("Jump");
        }
    }

    //left:-1,right 1
    private void Rotate(int R_dir)
    {
        if (90.0f - DifAngle > fRotateAngle)
        {
            transform.Rotate(0, R_dir * fRotateAngle, 0);
            DifAngle += fRotateAngle;
        }
        else
        {
            if (R_dir == 1)
            {
                m_right = false;
            }
            else if (R_dir == -1)
            {
                m_left = false;
            }
            transform.Rotate(0, R_dir * (90.0f - DifAngle), 0);
            dir += R_dir;
            dir += dir < 0 ? 4 : 0;
            dir %= 4;
            DifAngle = 0;
        }
    }
    //left:-1,right 1
    private void Horizontalmove(int dir)
    {
        if (Roadwidth - DifWidth > fMovewidth)
        {
            transform.position += transform.right * fMovewidth * (float)dir;
            DifWidth += fMovewidth;
        }
        else
        {
            if (dir == 1)
                m_rightmove = false;
            else if (dir == -1)
                m_leftmove = false;
            transform.position += transform.right * (Roadwidth - DifWidth) * (float)dir;
            DifWidth = 0;
        }
    }
    private void Difference()
    {
        Vector3 vectorDifference = (transform.position - BeginPos);
        moveDifference = (dir % 2 == 0) ? vectorDifference.z : vectorDifference.x % (3f * Roadwidth);
        fix = moveDifference < 0 ? -1 : 1;
        moveDifference = Mathf.Abs(moveDifference);
        //
        if (moveDifference <= (0.5f * Roadwidth))
        {
            moveDifference = 0f * Roadwidth - moveDifference;
        }
        else if (moveDifference <= (1.5f * Roadwidth))
        {
            moveDifference = 1f * Roadwidth - moveDifference;
        }
        else if (moveDifference <= (2.5f * Roadwidth))
        {
            moveDifference = 2f * Roadwidth - moveDifference;
        }
        else
        {
            moveDifference = 3f * Roadwidth - moveDifference;
        }
        moveDifference *= fix;
    }
    private void MoveDifference()
    {
        if (moveDifference * fix - fMovewidth > 0 && (m_right || m_left))
        {
            transform.position += ((dir % 2 == 0) ? Vector3.forward : Vector3.right) * fMovewidth * Roadwidth * fix;
            moveDifference -= (fMovewidth * fix);
        }
        else
        {
            transform.position += ((dir % 2 == 0) ? Vector3.forward : Vector3.right) * moveDifference * Roadwidth;
            m_move = false;
        }
    }
}
