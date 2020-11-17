using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ExampleTurretController : MonoBehaviour
{
    static Vector2 DirectionFromName(string name)
    {
        //Translate the name of the direction into the real thing
        switch (name.ToLower())
        {
            case "up":
                return Vector2.up;
            case "down":
                return Vector2.down;
            case "left":
                return Vector2.left;
            case "right":
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    Vector2 currentRotationDirection = Vector2.zero;
    Vector2 currentRotation = Vector2.zero;
    float spread = 2.5f;
    public float rotationSpeed = 10;
    public Text spreadText;

    ///<summary> Called by d-pad </summary>
    public void OnDPadDown(string d)
    {
        currentRotationDirection += DirectionFromName(d);
    }
    ///<summary> Called by d-pad </summary>
    public void OnDPadUp(string d)
    {
        currentRotationDirection -= DirectionFromName(d);
    }
    ///<summary> Called by spread slider</summary>
    public void OnSpreadChange(float newValue)
    {
        spread = newValue;
        spreadText.text = spread.ToString("0.00");

        //Update the spread of the crosshair
        CrosshairController.SetSpread(spread);
    }
    ///<summary> Called by fire button </summary>
    public void OnFire()
    {
        print("Firing!");
        //Fire a raycast with a random spread
        Vector3 direction = Quaternion.Euler(Random.Range(-spread, spread), Random.Range(-spread, spread), 0) * transform.forward;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, Mathf.Infinity, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            //Create an explosion at this position, effecting all rigidbodies in the scene
            //Because the demo is so small, this is okay
            foreach (var rb in FindObjectsOfType<Rigidbody>())
            {
                rb.AddExplosionForce(1000, hit.point, 2f);
            }
            //Create a marker at the hit position
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.transform.position = hit.point;
            marker.transform.rotation = Quaternion.LookRotation(hit.normal);
            marker.transform.localScale = Vector3.one * 0.1f;
            Destroy(marker.GetComponent<Collider>()); // No collider wanted on hit marker
            Destroy(marker, 1f); //Destroy the marker after a second
        }
    }
    private void Update()
    {
        //Update current rotation based on dpad keys pressed
        currentRotation += currentRotationDirection * Time.deltaTime * rotationSpeed;
        //Clamp up/down rotation
        currentRotation.y = Mathf.Clamp(currentRotation.y, -80f, 80f);
        //Weird euler positioning is due to rotation axis
        transform.rotation = Quaternion.Euler(-currentRotation.y, currentRotation.x, 0);
    }
}
