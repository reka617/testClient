using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    private float lastUpdateTime;
    private Vector3 Destination;
    private Vector3 Direction;
    private float Speed;
    private float interpTime = 0.2f;
    private float checkDistanceTime = 10.0f;

    private Coroutine interpDestinationCoroutine = null;

    IEnumerator InterpDestination()
    {
        Vector3 initPosition = transform.position;
        float lerp = 0.0f;
        while (true)
        {
            if (lerp >= 1.0f)
            {
                lerp = 1.0f;
            }
            
            transform.position = Vector3.Lerp(initPosition, Destination, lerp);
            if (lerp >= 1.0f)
            {
                interpDestinationCoroutine = null;
                yield break;
            }

            lerp += Time.deltaTime / interpTime;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
    }

    public void UpdatePlayerPosition(PlayerPosition p)
    {
        Destination = new Vector3(p.X, p.Y, p.Z);
        Direction = new Vector3(p.Fx, p.Fy, p.Fz);
        Speed = p.Speed;

        float distance = (transform.position - Destination).magnitude;
        if (interpDestinationCoroutine == null &&  distance > checkDistanceTime )
        {
            interpDestinationCoroutine = StartCoroutine(InterpDestination());
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (interpDestinationCoroutine != null)
        {
            return;
        }
        
        float timeSinceLastUpdate = Time.deltaTime;
        transform.position += Direction * (Speed * timeSinceLastUpdate);
    }
}