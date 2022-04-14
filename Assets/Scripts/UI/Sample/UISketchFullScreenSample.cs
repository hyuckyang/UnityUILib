using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    public class UISketchFullScreenSample : UISketch
    {
        public static string AssetPath => "UISketchFullScreenSample";
        protected override bool IsFullScreen => true;
    }    
}

