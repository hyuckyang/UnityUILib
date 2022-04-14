using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    public class SceneSample : MonoBehaviour
    {
        private void Awake()
        {
            UIManager.Instance.Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                UIManager.Instance.Open<UISketchSample>();
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                UIManager.Instance.Open<UISketchPoolSample>();
            }
            
            if (Input.GetKeyUp(KeyCode.E))
            {
                UIManager.Instance.Open<UISketchFullScreenSample>();
            }
        }
    }
}

