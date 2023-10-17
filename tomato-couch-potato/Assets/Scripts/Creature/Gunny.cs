using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using trrne.Teeth;
using UnityEngine;

namespace trrne.Body
{
    public class Gunny : Creature
    {
        (GameObject obj, LineRenderer line, float lineLength) gun;

        Vector3 direction = Vector100.x;
        float speed = 10f;

        Rigidbody2D rb;

        protected override void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            gun.obj = transform.GetChild(0).gameObject;
            gun.line = gun.obj.GetComponent<LineRenderer>();
        }

        protected override void Move()
        {
            ;
        }

        protected override void Behavior()
        {
            ;
        }

        void ADS()
        {
            gun.line.startColor = gun.line.endColor = Color.HSVToRGB(Time.time % 1, 1, 1);
        }

        public override UniTask Die()
        {
            throw new System.NotImplementedException();
        }
    }
}