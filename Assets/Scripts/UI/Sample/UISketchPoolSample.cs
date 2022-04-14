using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UILib
{
    public class UISketchPoolSample : UISketch
    {
        public static string AssetPath => "UISketchPoolSample";
        protected override bool IsPooling => true;
        protected override bool IsFullScreen => false;
    }

}
