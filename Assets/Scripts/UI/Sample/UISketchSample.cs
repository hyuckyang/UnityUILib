using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    public class UISketchSample : UISketch
    {
        /// <summary>
        /// 추후 번들 에 따른 경로가 들어가야 합니다.
        /// 지금은 임시로 리소스 경로를 따라기에 이름만 붙입니다.
        /// </summary>
        public static string AssetPath => "UISketchSample";
        protected override bool IsFullScreen => false;
    }
}
