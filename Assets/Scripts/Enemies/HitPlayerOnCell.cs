using UnityEngine;

public class HitPlayerOnCell : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == Player.playerObject)
        {
            Debug.Log("hit player");
            Player.hitPlayer(2);
        }
    }
}
