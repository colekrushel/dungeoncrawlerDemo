using UnityEngine;

//use as an animation event to trigger a projectile launch
public class LaunchProjectile : MonoBehaviour
{
    [SerializeField] GameObject projectile; //projectile to be launched
    [SerializeField] GameObject startpos; //gameobject to move the projectile to when spawned
    public void launch()
    {
        //projectile will be fired at the player by default
        GameObject proj = Instantiate(projectile);
        proj.transform.position = startpos.transform.position;
        proj.transform.LookAt(Player.playerObject.transform);
        //set projectile's initial position to be the enemy's position
        
    }

 
}
