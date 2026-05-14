using UnityEngine;

public class PlanetBehavior : MonoBehaviour
{
    public float weight = 600;
    public float range = 100;
    public LayerMask playerMask;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerBehavior player = null;
        if (Physics.OverlapSphere(transform.position, range, playerMask).Length > 0) player = Physics.OverlapSphere(transform.position, range, playerMask)[0].GetComponent<PlayerBehavior>();

        if (player != null) player.AddPlanet(this);
        else FindAnyObjectByType<PlayerBehavior>().RemovePlanet(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
