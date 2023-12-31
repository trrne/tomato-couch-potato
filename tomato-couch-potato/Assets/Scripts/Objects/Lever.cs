using trrne.Box;
using UnityEngine;

namespace trrne.Core
{
    public class Lever : Object
    {
        [SerializeField]
        GameObject[] gimmicks;

        [SerializeField]
        AudioClip[] sounds;

        AudioSource source;
        bool isActive = false;

        LeverFlag flag;

        protected override void Start()
        {
            base.Start();

            flag = transform.GetFromChild<LeverFlag>();
            source = Gobject.GetWithTag<AudioSource>(Config.Tags.MANAGER);

            sr.sprite = sprites[isActive ? 0 : 1];
        }

        protected override void Behavior()
        {
            if (!flag.IsHitting != false)
            {
                return;
            }

            if (isActive && Inputs.Down(Config.Keys.BUTTON))
            {
                source.TryPlayOneShot(sounds.Choice());
                sr.sprite = sprites[1];
                gimmicks.ForEach(gimmick =>
                {
                    if (gimmick.TryGetComponent(out IGimmick g))
                    {
                        g.On();
                    }
                });
                isActive = false;
            }
            else if (!isActive && Inputs.Down(Config.Keys.BUTTON))
            {
                source.TryPlayOneShot(sounds.Choice());
                sr.sprite = sprites[0];
                gimmicks.ForEach(gim =>
                {
                    if (gim.TryGetComponent(out IGimmick g))
                    {
                        g.Off();
                    }
                });
                isActive = true;
            }
        }
    }
}
