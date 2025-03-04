using UnityEngine;
using Fujin.Constants;
using Fujin.Player;

namespace Fujin.Mobs
{
    /// <summary>
    /// Is attached to the body of a simple rocket to send a signal of reaching a goal to PlayerPhysics upon colliding
    /// ... only as long as the playable character keeps their animation isNinjaing true.
    /// </summary>
    public class GoalRocketManager : MonoBehaviour
    {
        private PlayerPhysics player;
        private void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.CompareTag(GameObjectTag.Player))
            {
                player ??= col.gameObject.GetComponent<PlayerPhysics>();
                if (player.OnWall)
                {
                    player.ReachGoal();
                }
            }
        }
    }
}