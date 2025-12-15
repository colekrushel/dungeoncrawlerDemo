using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //script attached to projectile objects that just check for collision every frame.
    [SerializeField] float lifetime; //how long the projectile should exist before being destroyed ( in seconds?)
    [SerializeField] int damage;
    private int i = 0;
    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime * 5;
        if(lifetime < 0)
        {
            Destroy(gameObject);
        }
        //move projectile in right direction because its rotation is set at instantiation so that the right/x vector is looking at the player
        transform.position += transform.right * Time.deltaTime * 2;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject == Player.playerObject && i == 0)
        {
            Debug.Log("projectile collision with " + other.name);
            StartCoroutine(Reset());

        }
    }

    IEnumerator Reset()
    {
       
        yield return new WaitForEndOfFrame();
        i++;
        Player.hitPlayer(damage);
        Destroy(gameObject);
        

    }

}
