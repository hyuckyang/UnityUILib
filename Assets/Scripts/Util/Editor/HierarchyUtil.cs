using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UILib
{
    public static class HierarchyUtil
    {
          public static void CreateUISketch(MenuCommand menuCommand)
        {
            var go = CreateUISketch();
            
            Undo.RegisterCreatedObjectUndo((UnityEngine.Object)go, "Create " + go.name);
            CreateEventSystem(false);
        }
        
        [MenuItem("GameObject/UI/UISketch Camera")]
        public static void CreateUISketchWithCamera(MenuCommand menuCommand)
        { 
            var layer = LayerMask.NameToLayer("UI");
            var go = CreateUISketch();
            go.name = "UISketchWithCamera";
            
            // 카메라
            var goCam = new GameObject("Camera");
            goCam.layer = layer;
            
            goCam.transform.SetParentAndResetTransform(go.transform);
            goCam.transform.SetAsFirstSibling();
            var cam = goCam.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Depth;
            cam.cullingMask = 1 << 5;
            cam.orthographic = true;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            cam.useOcclusionCulling = false;
            cam.allowDynamicResolution = false;
            cam.allowHDR = false;
            cam.allowMSAA = false;

            var canvas = go.GetComponentInChildren<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
        }

        [MenuItem("GameObject/UI/UISketch")]
        private static GameObject CreateUISketch()
        {
            var layer = LayerMask.NameToLayer("UI");
            var go = new GameObject("UISketch");
            
            // 캔버스
            var goCanvas = new GameObject("Canvas");
            goCanvas.layer = layer;
            goCanvas.transform.SetParentAndResetTransform(go.transform);
            var canvas = goCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var canvasScaler = goCanvas.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            /*var gRaycaster =*/
            goCanvas.AddComponent<GraphicRaycaster>();
            
            // 컨텐츠
            // 실제 UI 객체들이 이 아래에서 붙어 제작되어야 합니다.
            var goContent = new GameObject("Contents");
            goContent.layer = layer;
            goContent.transform.SetParentAndResetTransform(goCanvas.transform);
            var rectContent = goContent.AddComponent<RectTransform>();
            rectContent.anchorMin = Vector2.zero;
            rectContent.anchorMax = Vector2.one;
            rectContent.pivot = Vector2.one * 0.5f;
            rectContent.offsetMin = new Vector2(0, 0);
            rectContent.offsetMax = new Vector2(0, 0);


            return go;
        }
        
        private static void CreateEventSystem(bool select, GameObject parent = null)
        {
            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                var child = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(child, parent);
                eventSystem = child.AddComponent<EventSystem>();
                child.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(child, "Create " + child.name);
            }
            if (!select || !(eventSystem != null))
                return;
            Selection.activeGameObject = eventSystem.gameObject;
        }
    }
}


