﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using trrne.Box;
using trrne.Brain;
using System.Collections.Generic;

namespace trrne.Core
{
    public enum Cause
    {
        None,
        Muscarine,  // 毒
        Fallen,     // 落下死
    }

    public enum Effect
    {
        Mirror,     // 操作左右反転
        Chain,      // ジャンプ不可
        Fetters,    // 移動速度低下
    }

    public class Player : MonoBehaviour, ICreature
    {
        [SerializeField]
        GameObject diefx;

        /// <summary> 操作フラグ </summary>
        public bool Controllable { get; set; }

        /// <summary> テレポート中か </summary>
        public bool IsTeleporting { get; set; }

        /// <summary> 死亡処理中か </summary>
        public bool IsDying { get; private set; } = false;

        int fxidx = -1;
        public bool[] Effectables { get; private set; }
        public bool[] EffectFlags { get; set; }
        int effectLength => new Effect().Length();

        (float basis, float max) speed => (20f, 10f);
        (float fetters, float floating, float move) red => (0.5f, 0.95f, 0.9f);
        (float floating, float basis) gscale => (3f, 1f);

        const float JUMP_POWER = 13f;

        public bool IsFloating { get; private set; }
        (bool ice, bool ground) on = (false, false);

        Rigidbody2D rb;
        Animator animator;
        Cam cam;
        PauseMenu menu;
        BoxCollider2D hitbox;

        public Vector3 Core { get; private set; }

        const float INPUT_TOLERANCE = 1 / 3;

        public Vector2 Checkpoint { get; private set; } = default;

        public void SetCheckpoint(Vector2 position) => Checkpoint = position;
        public void SetCheckpoint(float? x = null, float? y = null) => Checkpoint = new(x ?? transform.position.x, y ?? transform.position.y);

        public void ReturnToCheckpoint() => transform.position = Checkpoint;

        Vector2 reverse => new(-1, 1);

        Vector2 box;
        Vector2 hitboxSizeRatio => new(.8f, .1f);

        const int ANIMATION_DELAY = 1250;

        void Start()
        {
            menu = Gobject.GetWithTag<PauseMenu>(Constant.Tags.MANAGER);
            cam = Gobject.GetWithTag<Cam>(Constant.Tags.MAIN_CAMERA);
            animator = GetComponent<Animator>();
            hitbox = GetComponent<BoxCollider2D>();
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = gscale.basis;

            EffectFlags = new bool[effectLength].Set(false);
            Effectables = new bool[effectLength].Set(true);

            box = new(hitbox.bounds.size.x * hitboxSizeRatio.x, hitbox.bounds.size.y * hitboxSizeRatio.y);
        }

        void FixedUpdate()
        {
            Move();
        }

        void Update()
        {
            Def();
            Footer();
            Jump();
            Flip();
            Respawn();
            PunishFlagsUpdater();
        }

#if !DEBUG
        [SerializeField]
        Text floatingT;

        void LateUpdate()
        {
            floatingT.SetText($"OnIce: {on.ice}\nOnGround: {on.ground}");
        }
#endif

        void Def()
        {
            Core = transform.position + new Vector3(0, hitbox.bounds.size.y / 2);
            rb.bodyType = IsTeleporting ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
            rb.gravityScale = (IsFloating = !on.ground) ? gscale.floating : gscale.basis;
        }

        void Footer()
        {
            if (!Gobject.Boxcast(out var hit, transform.position, box, Constant.Layers.JUMPABLE))
            {
                on.ice = on.ground = false;
                return;
            }

            on.ground = hit.CompareLayer(Constant.Layers.JUMPABLE);
            on.ice = hit.CompareTag(Constant.Tags.ICE);
        }

#if DEBUG
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, box);
        }
#endif

        /// <summary>
        /// スペースでリスポーンする
        /// </summary>
        void Respawn()
        {
            if (!IsDying && Inputs.Down(Constant.Keys.RESPAWN))
            {
                ReturnToCheckpoint();
            }
        }

        public IEnumerator Punishment(float duration, Effect effect)
        {
            if (fxidx == -1 && Effectables[fxidx = (int)effect])
            {
                EffectFlags[fxidx] = true;
                yield return new WaitForSeconds(duration);
                EffectFlags[fxidx] = false;
                fxidx = -1;
            }
        }

        void PunishFlagsUpdater()
        {
            for (int i = 0; i < effectLength; ++i)
            {
                Effectables[i] = !EffectFlags[i];
            }
        }

        void Flip()
        {
            if (!Controllable || menu.IsPausing || IsTeleporting)
            {
                return;
            }

            string horizontal = EffectFlags[(int)Effect.Mirror] ? Constant.Keys.MIRRORED_HORIZONTAL : Constant.Keys.HORIZONTAL;
            if (Inputs.Down(horizontal))
            {
                int pre = MathF.Sign(transform.localScale.x);
                float haxis = Input.GetAxisRaw(horizontal);
                if (MathF.Sign(haxis) != 0 && MathF.Sign(haxis) != pre)
                {
                    transform.localScale *= reverse;
                }
            }
        }

        void Jump()
        {
            if (!Controllable || EffectFlags[(int)Effect.Chain] || IsTeleporting)
            {
                return;
            }

            if (!on.ground)
            {
                animator.SetBool(Constant.Animations.Jump, true);
                return;
            }

            if (Inputs.Down(Constant.Keys.JUMP))
            {
                rb.velocity += JUMP_POWER * Vec.Y.ToV2();
            }
            animator.SetBool(Constant.Animations.Jump, false);
        }

        /// <summary>
        /// 移動
        /// </summary>
        void Move()
        {
            if (!Controllable || IsTeleporting)
            {
                return;
            }

            bool walkAnimationFlag = on.ground && Inputs.Pressed(Constant.Keys.HORIZONTAL);
            animator.SetBool(Constant.Animations.Walk, walkAnimationFlag);

            var horizon = EffectFlags[(int)Effect.Mirror] ? Constant.Keys.MIRRORED_HORIZONTAL : Constant.Keys.HORIZONTAL;
            var move = Vec.MakeVec2(x: Input.GetAxisRaw(horizon));

            // 入力がtolerance以下、氷に乗っていない
            if (move.magnitude <= INPUT_TOLERANCE && !on.ice)
            {
                // x軸の速度をspeed.reduction倍
                rb.SetVelocity(x: rb.velocity.x * red.move);
            }

            // x軸の速度を制限
            if (rb.bodyType != RigidbodyType2D.Static)
            {
                float limit = Shorthand.L1ne(() =>
                {
                    if (EffectFlags[(int)Effect.Fetters])
                    {
                        return speed.max * red.fetters;
                    }
                    else if (on.ice)
                    {
                        return speed.max * 2;
                    }
                    return speed.max;
                });
                rb.ClampVelocity(x: (-limit, limit));
            }

            // 浮いていたら移動速度低下 
            rb.velocity += Time.fixedDeltaTime * speed.basis * (IsFloating ? red.floating : 1f) * move;
        }

        /// <summary>
        /// 成仏
        /// </summary>
        public async UniTask Die(Cause cause = Cause.None)
        {
            if (IsDying)
            {
                return;
            }

            // 敵への当たり判定は除外
            hitbox.excludeLayers += Constant.Layers.CREATURE;

            IsDying = true;
            Controllable = cam.Followable = false;

            diefx.TryInstantiate(transform.position);

            switch (cause)
            {
                case Cause.None:
                    break;
                case Cause.Muscarine:
                    rb.velocity = Vector2.zero;
                    animator.Play(Constant.Animations.Venomed);
                    break;
                case Cause.Fallen:
                    animator.Play(Constant.Animations.Die);
                    break;
            }

            await UniTask.Delay(ANIMATION_DELAY);

            // mend
            Gobject.Finds<Carrot>().ForEach(carrot => carrot.Mendable.If(carrot.Mend));
            Gobject.Finds<MoroiFloor>().ForEach(moro => moro.Mendable.If(moro.Mend));

            ReturnToCheckpoint();
            animator.StopPlayback();

            hitbox.excludeLayers -= Constant.Layers.CREATURE;

            cam.Followable = Controllable = true;
            IsDying = false;

            // エフェクトを削除
            EffectFlags.Set(false);
        }

        public async UniTask Die() => await Die(Cause.None);
    }
}