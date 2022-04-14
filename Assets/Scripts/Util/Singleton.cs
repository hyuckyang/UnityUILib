using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    /// <summary>
    /// 참조
    /// https://stackoverflow.com/questions/2550925/singleton-by-jon-skeet-clarification
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        protected Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Nested.Inst;
                    _instance.Init();
                }

                return _instance;
            }
        }

        protected virtual void Init()
        {
            
        }

        private static class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly T Inst = new T();
        }
    }
}

