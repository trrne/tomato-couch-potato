using trrne.Box;
using UnityEngine;

namespace trrne.Core
{
    public class Mama : MonoBehaviour
    {
        [SerializeField]
        MamaFlag flag;

        [SerializeField]
        GameObject[] fires;

        [SerializeField]
        float fireRapidRate = 2f;
        float fireRapidTimer = 0;

        Player player;

        [SerializeField]
        GameObject[] eyes;
        Vector3[] inits;
        // Vector3[] directions;

        /// <summary>
        /// 黒目の可動域
        /// </summary>
        const float EYE_BUMP = 0.25f;

        void Start()
        {
            player = Gobject.GetWithTag<Player>(Config.Tags.PLAYER);

            inits = new Vector3[eyes.Length];
            for (int i = 0; i < eyes.Length; i++)
            {
                inits[i] = eyes[i].transform.position;
            }
        }

        void Update()
        {
            Vector3[] directions = {
                player.Core - eyes[0].transform.position,
                player.Core - eyes[1].transform.position
            };

            // player within range
            if (!flag.IsInsideRange)
            {
                return;
            }

            for (int i = 0; i < eyes.Length; i++)
            {
                eyes[i].transform.position = inits[i] + directions[i].normalized * EYE_BUMP;
            }

            // the fires generate if timer value is upper than rapid rate 
            if ((fireRapidTimer += Time.deltaTime) >= fireRapidRate)
            {
                fires.TryInstantiate(transform.position);
                fireRapidTimer = 0;
            }
        }
    }
}
