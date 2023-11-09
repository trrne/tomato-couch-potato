using System.Collections;
using trrne.Box;
using UnityEngine;

namespace trrne.Brain
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField]
        GameObject panel;

        CanvasGroup canvas;

        public bool IsPausing { get; private set; }

        // (float speed, bool during) fade = (10f, false);
        float fadeSpeed = 10f;
        bool isFading = false;

        void Start()
        {
            canvas = panel.GetComponent<CanvasGroup>();
        }

        void Update()
        {
            IsPausing = canvas.alpha >= .5f;
            PanelControl();
        }

        public void Active() => State(true);
        public void Inactive() => State(false);

        void State(bool active)
        {
            FadingHandle(active);
            App.SetTimeScale(active ? 0 : 1);
        }

        void PanelControl()
        {
            if (!Inputs.Down(Constant.Keys.Pause))
            {
                return;
            }

            if (IsPausing)
            {
                Inactive();
            }
            else
            {
                Active();
            }
        }

        void FadingHandle(bool fin)
        {
            if (isFading)
            {
                return;
            }

            StartCoroutine(Fader(fin));
        }

        // フェード処理
        IEnumerator Fader(bool fin)
        {
            isFading = true;
            float alpha = fin ? 0f : 1;
            while (alpha.IsCaged(0, 1))
            {
                yield return null;

                alpha = fin ?
                    alpha += Time.unscaledDeltaTime * fadeSpeed :
                    alpha -= Time.unscaledDeltaTime * fadeSpeed;

                canvas.alpha = alpha;
            }
            // フェード処理終了
            isFading = false;
        }
    }
}
