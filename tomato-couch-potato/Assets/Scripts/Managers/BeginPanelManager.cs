﻿using System.Collections;
using trrne.Core;
using trrne.Box;
using UnityEngine;

namespace trrne.Brain
{
    public class BeginPanelManager : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup canvas;

        const float SHOW_TIME = 3, FADE_SPEED = 10;
        readonly Stopwatch sw = new();

        Player player;

        void Start()
        {
            canvas.alpha = 1;
            sw.Start();

            player = Gobject.GetWithTag<Player>(Config.Tags.PLAYER);
            player.Controllable = false;
        }

        void Update()
        {
            if (sw.Sf() >= SHOW_TIME)
            {
                sw.Reset();
                StartCoroutine(FadeOut());
            }
        }

        IEnumerator FadeOut()
        {
            float alpha = 1f;
            while (alpha >= 0)
            {
                yield return null;
                alpha -= Time.unscaledDeltaTime * FADE_SPEED;
                canvas.alpha = alpha;
            }
            canvas.alpha = 0;
            yield return new WaitForSeconds(.5f);
            player.Controllable = true;
        }
    }
}