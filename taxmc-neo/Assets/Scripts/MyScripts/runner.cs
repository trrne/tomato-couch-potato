﻿using System;
using System.Collections;
using UnityEngine;

namespace trrne.utils
{
    public class Runner
    {
        bool flag0;
        /// <summary>actionを一回実行</summary>
        public void RunOnce(params Action[] actions)
        {
            try
            {
                if (flag0) { return; }

                foreach (var action in actions)
                {
                    action();
                }

                flag0 = true;
            }
            catch (Exception e) { MonoBehaviour.print(e.Message); }
        }

        public static T Function<T>(Func<T> func) => func();

        readonly static Stopwatch bookingSW = new(true);
        public static void Book(float time, Action action)
        {
            if (bookingSW.sf >= time)
            {
                action();
                bookingSW.Rubbish();
            }
        }

        /// <summary>
        /// 何もしないとき用
        /// </summary>
        public static void NothingSpecial() {; }
    }
}