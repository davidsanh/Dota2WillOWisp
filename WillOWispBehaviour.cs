using System.Linq;
using UnityEngine;

public class WillOWispBehaviour : MonoBehaviour
{
    public string[] tagsToCheck; //The tag to check, example; Enemy, Environment, Default, etc.
    public int flickerAmount = 5; //The times it will flicker
    public float movementSpeed = 0.5f; //Affected unit movement speed towards wisp
    public float rotationSpeed = 5; //Affected unit rotation speed towards wisp
    public float radius = 10; //Area of effect, can be visualize using gizmos
    public float stopRange = 1.5f; //How close to the wisp the affected unity will stop
    public float flickerDuration = 2; //Duration of wisp flicker
    public float flickerRestDuration = 3; //Duration between flickers
    public float idleFloatDistance = 0.5f; //Floating; Y distance for the wisp to move up and down
    private bool flickering = false, isAlive; //These bools control our loops
    private float flickerDurationCounter, flickerRestDurationCounter; //More hidden counter/timer
    private ParticleSystem particle; //Wisp particle
    private Transform wisp; //The transform to move up and down
    private Vector3 upPos, downPos; //Saved positions to move our wisp transform

    private void Start()
    {
        wisp = transform.Find("Wisp"); //Assign our wisp transform

        //Assign the locations for our wisp to move up and down
        upPos = wisp.position;
        upPos.y += idleFloatDistance;
        downPos = wisp.position;
        downPos.y -= idleFloatDistance;

        particle = transform.Find("Wisp/Effect").GetComponent<ParticleSystem>(); //Assign our particle system

        //Making sure our values are set correctly
        flickerDurationCounter = flickerDuration;
        isAlive = true;
    }

    void Update()
    {
        if (isAlive) //If our flicker amount is bigger than 0 then we are alive
        {
            //Loop that moves the wisp transform up and down
            wisp.position = Vector3.Lerp(upPos, downPos, (Mathf.Sin(1 * Time.time) + 1.0f) / 2.0f);

            if (flickerAmount > 0)
            {
                if (flickering)
                {
                    Flicker(transform.position); //Call our function that moves affected units towards wisp
                }
                else
                {
                    if(flickerRestDurationCounter > 0)
                    {
                        flickerRestDurationCounter -= Time.deltaTime;
                    }
                    else
                    {
                        //Set our particle's rate over time
                        var emi = particle.emission;
                        emi.rateOverTime = 20;

                        //Set our bool to true so that we start flickering
                        flickering = true;
                        flickerDurationCounter = flickerDuration;
                    }
                }
            }
            else { isAlive = false; }
        }
        else
        {
            Destroy(gameObject); //If we are not alive then we destroy ourselves
        }
    }

    void Flicker(Vector3 destination)
    {
        if (flickerDurationCounter > 0)
        {
            flickerDurationCounter -= Time.deltaTime;
            flickering = true;

            //Create a sphere around location using our radius
            Collider[] objectsInRange = Physics.OverlapSphere(transform.position, radius);

            //We get all colliders that overlap our sphere cast
            foreach (Collider col in objectsInRange)
            {
                if (tagsToCheck.Contains(col.tag)) //Check our tags before moving and rotating affected units
                {
                    //Save the distance between our wisp and the affected unity in a temporary variable
                    var distance = Vector3.Distance(col.transform.position, transform.position); 
                    if (distance > stopRange)
                    {
                        //Movement
                        col.transform.position = Vector3.MoveTowards(col.transform.position, destination, movementSpeed * Time.deltaTime);
                    }

                    //Rotation
                    Vector3 dir = transform.position - col.transform.position;
                    dir.y = 0; // keep the direction strictly horizontal
                    Quaternion rot = Quaternion.LookRotation(dir);
                    col.transform.rotation = Quaternion.Slerp(col.transform.rotation, rot, rotationSpeed * Time.deltaTime);
                }
            }
        }
        else
        {
            //Set our particle's rate over time
            var emi = particle.emission;
            emi.rateOverTime = 0;

            //Reset our loop
            flickering = false;
            //Set our resting between flicker counter
            flickerRestDurationCounter = flickerRestDuration;
            //Lower the flicker amount
            flickerAmount -= 1;
        }
    }

    private void OnDrawGizmos() //Visualize our area of effect using gizmos
    {
        if (flickering)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}