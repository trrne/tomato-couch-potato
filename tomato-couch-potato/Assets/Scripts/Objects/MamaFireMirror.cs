using Cysharp.Threading.Tasks;
using UnityEngine;

namespace trrne.Core
{
    public class MamaFireMirror : MamaFire
    {
        protected override void Start()
        {
            base.Start();
            isTracking = false;
        }

        protected override void Movement() => transform.Translate(Time.deltaTime * speed * direction);
        protected override async UniTask Punishment(Player player) => await player.Punishment(effectDuration, Effect.Mirror);
    }
}
