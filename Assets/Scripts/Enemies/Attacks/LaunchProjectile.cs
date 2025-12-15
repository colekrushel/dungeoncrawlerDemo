using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
        //proj.transform.LookAt(Player.playerObject.transform);
        //rotate because the stinger's x axis is where its point is
        Vector3 directionToTarget = Player.playerObject.transform.position - proj.transform.position;
        proj.transform.rotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(0, -90, 0);

    }

 
}
