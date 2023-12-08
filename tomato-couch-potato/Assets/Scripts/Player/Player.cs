﻿using System.Transactions;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using trrne.Box;
using trrne.Brain;
using UnityEngine.UI;

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
        Mirror,
        Chain,
        Fetters,
    }

    public class Player : MonoBehaviour, ICreature
    {
        [SerializeField]
        GameObject diefx;

        public bool Controllable { get; set; }
        public bool IsMoveKeyDetecting { get; private set; }
        public bool IsTeleporting { get; set; }
        public bool IsDying { get; private set; } = false;

        int fxidx = -1;
        public bool[] Effectables { get; private set; }
        public bool[] EffectFlags { get; set; }

        readonly (float basis, float max) speed = (
            basis: 20,
            max: 10
        );

        readonly (float fetters, float floating, float move) reduction = (
            fetters: 0.5f,
            floating: 0.95f,
            move: 0.9f
        );

        readonly (float floating, float basis) gscale = (
            floating: 3f,
            basis: 1f
        );
        const float JumpPower = 13f;

        public bool IsFloating { get; private set; }
        public Vector2 Velocity { get; private set; }

        Rigidbody2D rb;
        Animator animator;
        Cam cam;
        PauseMenu menu;
        BoxCollider2D hitbox;

        public Vector3 Core { get; private set; }

        const float InputTolerance = 0.33f;

        public Vector2 Checkpoint { get; private set; } = Vector2.zero;

        public void SetCheckpoint(Vector2 position) => Checkpoint = position;
        public void SetCheckpoint(float? x = null, float? y = null)
        => Checkpoint = new(x ?? transform.position.x, y ?? transform.position.y);

        public void ReturnToCheckpoint() => transform.position = Checkpoint;

        readonly Vector2 reverse = new(-1, 1);

        Vector2 box;
        (bool ice, bool ground) on = (false, false);

        void Start()
        {
            menu = Gobject.GetWithTag<PauseMenu>(Config.Tags.Manager);
            cam = Gobject.GetWithTag<Cam>(Config.Tags.MainCamera);
            animator = GetComponent<Animator>();
            hitbox = GetComponent<BoxCollider2D>();
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = gscale.basis;

            int length = new Effect().Length();
            EffectFlags = new bool[length];
            Effectables = new bool[length];
            for (int i = 0; i < EffectFlags.Length; i++)
            {
                EffectFlags[i] = false;
                Effectables[i] = true;
            }

            box.x = hitbox.bounds.size.x * 0.8f;
            box.y = hitbox.bounds.size.y * 0.1f;
        }

        void FixedUpdate()
        {
            Move();
        }

        [SerializeField]
        Text floatingT;

        void Update()
        {
            Def();
            Footer();
            Jump();
            Flip();
            Respawn();
            PunishFlagsUpdater();
        }

        void LateUpdate()
        {
            floatingT.SetText($"{nameof(on.ground)}: {on.ground}\n{nameof(IsFloating)}: {IsFloating}");
        }

        void Def()
        {
            Core = transform.position + new Vector3(0, hitbox.bounds.size.y / 2);
            rb.bodyType = IsTeleporting ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
            IsFloating = !on.ground;
            rb.gravityScale = IsFloating ? gscale.floating : gscale.basis;
        }

        void Footer()
        {
            // var hit = Gobject.Boxcast(transform.position, box, );
            if (!Gobject.Boxcast(out var hit, transform.position, box, Config.Layers.Jumpable))
            {
                on.ice = on.ground = false;
                return;
            }

            if (hit.CompareLayer(Config.Layers.Jumpable))
            {
                on.ground = true;
            }
            if (hit.CompareTag(Config.Tags.Ice))
            {
                on.ice = true;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, box);
        }

        /// <summary>
        /// スペースでリスポーンする
        /// </summary>
        void Respawn()
        {
            if (IsDying)
            {
                return;
            }

            if (Inputs.Down(Config.Keys.Respawn))
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
            for (int i = 0; i < EffectFlags.Length; i++)
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

            var horizontal = EffectFlags[(int)Effect.Mirror] ?
                Config.Keys.MirroredHorizontal : Config.Keys.Horizontal;
            if (Input.GetButtonDown(horizontal))
            {
                int current = Maths.Sign(transform.localScale.x);
                switch (Mathf.Sign(Input.GetAxisRaw(horizontal)))
                {
                    case 1:
                        if (current != 1)
                        {
                            transform.localScale *= reverse;
                        }
                        break;
                    case -1:
                        if (current != -1)
                        {
                            transform.localScale *= reverse;
                        }
                        break;
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
                animator.SetBool(Config.Animations.Jump, true);
                return;
            }

            if (Inputs.Down(Config.Keys.Jump))
            {
                rb.velocity += JumpPower * Vec.VY.ToV2();
            }
            animator.SetBool(Config.Animations.Jump, false);
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

            bool walk = Inputs.Pressed(Config.Keys.Horizontal) && on.ground;
            animator.SetBool(Config.Animations.Walk, walk);

            string horizontal = EffectFlags[(int)Effect.Mirror] ?
                Config.Keys.MirroredHorizontal : Config.Keys.Horizontal;
            Vector2 move = Input.GetAxisRaw(horizontal) * Vec.VX;

            // 入力がtolerance以下、氷に乗っていない
            if (move.magnitude <= InputTolerance && !on.ice)
            {
                // x軸の速度をspeed.reduction倍
                rb.SetVelocity(x: rb.velocity.x * reduction.move);
            }

            float limit = Shorthand.L1ne(() =>
            {
                if (EffectFlags[(int)Effect.Fetters])
                {
                    return speed.max * reduction.fetters;
                }
                else if (on.ice)
                {
                    return speed.max * 2;
                }
                return speed.max;
            });

            // x軸の速度を制限
            if (rb.bodyType != RigidbodyType2D.Static)
            {
                rb.ClampVelocity(x: (-limit, limit));
            }

            // 浮いていたら移動速度低下
            float scalar = IsFloating ? reduction.floating : 1f;
            rb.velocity += Time.fixedDeltaTime * speed.basis * scalar * move;
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
            hitbox.excludeLayers += Config.Layers.Creature;

            IsDying = true;
            Controllable = cam.Followable = false;

            diefx.TryInstantiate(transform.position);

            switch (cause)
            {
                case Cause.None:
                    break;
                case Cause.Muscarine:
                    rb.velocity = Vector2.zero;
                    animator.Play(Config.Animations.Venomed);
                    break;
                case Cause.Fallen:
                    animator.Play(Config.Animations.Die);
                    break;
            }

            await UniTask.Delay(1250);

            // mend carrots
            Gobject.Finds<Carrot>().ForEach(c => c.Mendable.If(c.Mend));

            ReturnToCheckpoint();
            animator.StopPlayback();

            hitbox.excludeLayers -= Config.Layers.Creature;

            cam.Followable = Controllable = true;
            IsDying = false;

            // エフェクトを削除
            for (int i = 0; i < EffectFlags.Length; i++)
            {
                EffectFlags[i] = false;
            }
        }

        public async UniTask Die() => await Die(Cause.None);
    }
}