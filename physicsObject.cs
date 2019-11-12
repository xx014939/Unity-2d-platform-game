using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class physicsObject : MonoBehaviour
{

    protected Vector2 targetVelocity;


    public float MinGroundNormalY = 0.65f;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    public float gravityModifier = 1f;
    protected const float minMoveDistance = 0.001f;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected const float ShellRadius = 0.01f;
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
    protected bool grounded;
    protected Vector2 GroundNormal;
    

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D> ();
    }

    // Start is called before the first frame update
    void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    // Update is called once per frame
    void Update()
    {
        targetVelocity = Vector2.zero;
        ComputeVelocity();


    }

    protected virtual void ComputeVelocity()
    {

    }


    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;
        if(distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + ShellRadius);
            hitBufferList.Clear();

            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for(int i = 0; i < hitBufferList.Count; i++)
            {
                Vector2 CurrentNormal = hitBufferList[i].normal;
                if(CurrentNormal.y > MinGroundNormalY)
                {
                    grounded = true;
                    if (yMovement)
                    {
                        GroundNormal = CurrentNormal;
                        CurrentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(velocity, CurrentNormal);
                if (projection < 0)
                {
                    velocity = velocity - projection * CurrentNormal;
                }

                float modifiedDistance = hitBufferList[i].distance - ShellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance; 
               
            }

        }

        rb2d.position = rb2d.position + move.normalized * distance ;

    }

    void FixedUpdate()
    {
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;

        velocity.x = targetVelocity.x;

        grounded = false;

        Vector2 deltaPosition = velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2(GroundNormal.y, - GroundNormal.x);

        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move, true);

        move = Vector2.up * deltaPosition.y;

        Movement(move, true);

    }


}
