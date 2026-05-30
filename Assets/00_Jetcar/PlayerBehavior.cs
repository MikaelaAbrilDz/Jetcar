using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerBehavior : MonoBehaviour
{
    Rigidbody rb;
    float carMass;
    public List<PlanetBehavior> planets = new List<PlanetBehavior>();
    PlanetBehavior mainPlanet;
    Vector3 totalGravity;
    Vector3 aligningRotation;
    float direction;
    float gravityAddition;
    float turboPower = 1000;
    bool gas, turbo;
    [SerializeField] LayerMask planetMask;


    public int stars = 0;

    [SerializeField] private Animator animator;

    private bool collected = false;

    PlanetBehavior iMainPlanet
    {
        get
        {
            return mainPlanet;
        }
        set
        {
            mainPlanet = value;
        }
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carMass = rb.mass;
        rb.centerOfMass -= transform.up;
    }

    void Update()
    {
        totalGravity = Vector3.zero;
        PlanetBehavior mPlanet = planets[0];
        foreach (PlanetBehavior p in planets)
        {
            if (((carMass * p.weight) / Mathf.Pow((p.transform.position - transform.position).magnitude, 2)) > ((carMass * mPlanet.weight) / Mathf.Pow((mPlanet.transform.position - transform.position).magnitude, 2)))
            {
                mPlanet = p;
            }
        }
        iMainPlanet = mPlanet;
        totalGravity = (iMainPlanet.transform.position - transform.position) * ((carMass * iMainPlanet.weight) / Mathf.Pow((iMainPlanet.transform.position - transform.position).magnitude, 2));
        transform.parent = iMainPlanet.transform;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.position - iMainPlanet.transform.position, out hit, Mathf.Infinity, planetMask) && (transform.up - hit.normal).magnitude > 0.01f)
        {
            Vector3 directionOfMovement = transform.up - (transform.position - mainPlanet.transform.position);
            Vector3 axis = Vector3.Cross(transform.up, (transform.position - mainPlanet.transform.position).normalized).normalized;
            float angle = Mathf.Acos(Vector3.Dot(transform.up, (transform.position - mainPlanet.transform.position).normalized));
            aligningRotation = axis * angle * 50;
        }
        else if (transform.up != (transform.position - mainPlanet.transform.position).normalized)
        {
            aligningRotation *= 0.1f;
        }

        float speed;
        if (gas && rb.linearVelocity.magnitude < 20 && Physics.Raycast(transform.position, -transform.up, 1, planetMask)) speed = totalGravity.magnitude * 0.75f;
        else speed = 0;

        float turboSpeed;
        if (turbo) turboSpeed = turboPower;
        else turboSpeed = 0;


        totalGravity = totalGravity * (1 + gravityAddition);


    }

    private void FixedUpdate()
    {
        float speed;
        if (gas && rb.linearVelocity.magnitude < 20 && Physics.Raycast(transform.position, -transform.up, 1, planetMask)) speed = totalGravity.magnitude * 1.5f;
        else speed = 0;

        print(speed);

        float turboSpeed;
        if (turbo) turboSpeed = turboPower;
        else turboSpeed = 0;


        totalGravity = totalGravity * (1 + gravityAddition);


        rb.AddForce(totalGravity + (-(transform.up * totalGravity.magnitude * 0.15f) + (transform.forward * speed)) + (transform.up * 0.45f + transform.forward).normalized * turboSpeed);

        rb.AddTorque((transform.up * direction * 300) + aligningRotation);
    }

    public void Gas(InputAction.CallbackContext context)
    {
        if (context.performed) gas = true;
        if (context.canceled) gas = false;
    }
    public void Turbo(InputAction.CallbackContext context)
    {
        if (context.performed) turbo = true;
        if (context.canceled) turbo = false;
    }
    public void ChangeDirection(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<float>();
    }
    public void ChangeGravity(InputAction.CallbackContext context)
    {
        if (context.performed) gravityAddition = 5;
        if (context.canceled) gravityAddition = 0;
    }
    public void AddPlanet(PlanetBehavior p)
    {
        if (!planets.Contains(p)) planets.Add(p);
    }
    public void RemovePlanet(PlanetBehavior p)
    {
        if (planets.Contains(p) && planets.Count > 1) planets.Remove(p);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, totalGravity);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlanetBehavior>()) rb.linearVelocity *= 0.1f;
    }

    public void AddStar()
    {
        stars++;
        StarManager.Instance.AddStar();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Star"))
        {
            AddStar();

            Animator anim = other.GetComponentInChildren<Animator>();

            if (anim != null)
            {
                
                anim.SetTrigger("Collected");

                
                other.GetComponent<Collider>().enabled = false;

                
                Destroy(other.gameObject, 1f);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }




}
