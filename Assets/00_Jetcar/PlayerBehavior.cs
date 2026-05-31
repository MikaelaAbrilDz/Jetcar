using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

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
    bool gas, backGas, turbo, isDead;
    float speed, turboSpeed;
    int speedFwd = 1;
    [SerializeField] LayerMask planetMask;
    [SerializeField] Transform blackHole;
    [SerializeField] CinemachineCamera cam;

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
        if (isDead)
        {
            return;
        }
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
        print(totalGravity.magnitude);
        if (totalGravity.magnitude < 50)
        {
            StartCoroutine(Die());
        }

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


        if ((gas || backGas) && rb.linearVelocity.magnitude < 50 && Physics.Raycast(transform.position, -transform.up, 5, planetMask)) speed = speedFwd * totalGravity.magnitude * 0.75f;
        else speed = 0;

        if (turbo) turboSpeed = turboPower;
        else turboSpeed = 0;


        totalGravity = totalGravity * (1 + gravityAddition);


    }

    private void FixedUpdate()
    {
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
    public void GoBack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            backGas = true;
            speedFwd = -1;
        }
        if (context.canceled)
        {
            backGas = false;
            speedFwd = 1;
        }
    }
    public void AddPlanet(PlanetBehavior p)
    {
        if (!planets.Contains(p)) planets.Add(p);
    }
    public void RemovePlanet(PlanetBehavior p)
    {
        if (planets.Contains(p) && planets.Count > 1) planets.Remove(p);
    }
    public void AddStar()
    {
        stars++;
        StarManager.Instance.AddStar();
    }
    IEnumerator Die()
    {
        isDead = true;
        cam.Priority = -1;
        yield return new WaitForSeconds(5f);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        speed = 0;
        turboSpeed = 0;
        totalGravity = (blackHole.position - transform.position) * ((carMass * 9999) / Mathf.Pow((blackHole.position - transform.position).magnitude, 2));
        transform.position = blackHole.position + Vector3.up * 50;
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
