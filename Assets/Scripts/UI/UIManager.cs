using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Object = UnityEngine.Object;

namespace UILib
{
    public enum UILayer
    {
        Content, // Camera + Canvas
        Popup, // Canvas
        Toast, // Canvas
    }

    public enum UIRoot
    {
        Content = UILayer.Content,
        Popup = UILayer.Popup,
        Toast = UILayer.Toast,
        Pool
    }

    public partial class UIManager : Singleton<UIManager>, IDisposable
    {
        public class UISketchStack : IDisposable
        {
            private UILayer _layer;
            private UIManager _uiManager = null;
            private int _increaseDepth = 0;
            private readonly List<UISketch> _stack = new List<UISketch>();

            public int StackCount => _stack.Count;
            public UISketch LastedSketch => StackCount > 0 ? _stack[StackCount - 1] : null;

            public UISketchStack(UILayer layer, UIManager manager)
            {
                _layer = layer;
                _uiManager = manager;
                _increaseDepth = 0;
            }

            public void Dispose()
            {

            }

            public void Clear()
            {
                for (var i = _stack.Count - 1; i >= 0; i--)
                {
                    var sketch = _stack[i];
                    if (sketch != null)
                        sketch.OnClose();
                }
              
                _stack.Clear();
            }

            public void Add(bool add, UISketch sketch)
            {
                if (add)
                {
                    _stack.Add(sketch);
                }
                else
                {
                    // pop.
                    if (_stack.Count > 0)
                    {
                        _stack.Remove(sketch);
                    }
                }

                // Depth 설정
                var sketchDepth = sketch.SortSketch(add, _increaseDepth);
                if (add)
                    _increaseDepth += sketchDepth;
                else
                    _increaseDepth -= sketchDepth;
            }
            
            public int IndexOf(UISketch sketch)
            {
                return _stack.IndexOf(sketch);
            }
            
            public UISketch FindSketch(int idx)
            {
                if (idx >= 0 && _stack.Count > idx)
                {
                    return _stack[idx];
                }

                return null;
            }

            public T FindSketch<T>() where T : UISketch
            {
                if (StackCount == 0)
                {
                    return null;
                }

                foreach (var bh in _stack)
                {
                    if (bh == null)
                        continue;

                    if (bh is T t)
                        return t;
                }

                return null;
            }
        }

        private class UISketchPool : IDisposable
        {
            private UIManager _uiManager = null;
            private readonly Dictionary<Type, Queue<UISketch>> _pool = new Dictionary<Type, Queue<UISketch>>();

            public UISketchPool(UIManager manager)
            {
                _uiManager = manager;
            }

            public void Dispose()
            {
                Clear();
            }

            public void Clear()
            {
                // 그냥 삭제
                foreach (var pool in _pool)
                {
                    var queue = pool.Value;
                    foreach (var sketch in queue)
                    {
                        UnityEngine.Object.Destroy(sketch.gameObject);
                    }

                    queue.Clear();
                }

                _pool.Clear();
            }

            public void Add(UISketch sketch)
            {
                var t = sketch.GetType();
                if (!_pool.ContainsKey(t))
                {
                    _pool.Add(t, new Queue<UISketch>());
                }

                _pool[t].Enqueue(sketch);
            }

            public UISketch GetSketch(Type t)
            {
                if (!_pool.ContainsKey(t)) 
                    return null;
                
                return _pool[t].Count > 0 ? _pool[t].Dequeue() : null;
            }

            public UISketch GetSketch<T>() where T : UISketch
            {
                return GetSketch(typeof(T));
            }
        }

        private Dictionary<UIRoot, GameObject> _dicRoot = new Dictionary<UIRoot, GameObject>();
        private Dictionary<UILayer, UISketchStack> _dicStack = new Dictionary<UILayer, UISketchStack>();
        private UISketchPool _sketchPool = null;

        protected override void Init()
        {
            _sketchPool = new UISketchPool(this);
        }

        public void Dispose()
        {

        }

        public void Refresh()
        {
            var goParent = new GameObject("UIRoot");
            goParent.transform.ResetTransform();

            foreach (UIRoot root in Enum.GetValues(typeof(UIRoot)))
            {
                var idx = (int)root;
                var go = new GameObject() { name = $"UI{root.ToString()}" };
                go.transform.SetParentAndResetTransform(goParent.transform);
                go.transform.SetSiblingIndex(idx);
                _dicRoot.Add(root, go);
            }
        }

        // 씬 전환 등 , 내부 데이터 초기화가 필요할 때
        public void Clear()
        {
            _dicRoot.Clear();
            _sketchPool.Clear();
        }

        /// <summary>
        /// Asset Bundle Rule -> Path / Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Open<T>() where T : UISketch
        {
            var assetName = GetPrefabAssetName<T>();
            var assetPath = GetPrefabAssetPath<T>();
            var sketchType = typeof(T);
            var sketch = CreateSketch(sketchType, assetPath, assetName);
            return sketch != null ? sketch as T : null;
        }

        public T Open<T>(string path) where T : UISketch
        {
            var sketchType = typeof(T);
            var assetName = GetPrefabAssetName(path);
            var sketch = CreateSketch(sketchType, path, assetName);
            return sketch != null ? sketch as T : null;
        }

        public T Open<T>(string assetPath, string assetName) where T : UISketch
        {
            var sketchType = typeof(T);
            var sketch = CreateSketch(sketchType, assetPath, assetName);
            return sketch != null ? sketch as T : null;
        }

        public UISketch Open(string sketchName)
        {
            var sketchType = Type.GetType($"UILib.{sketchName}");
            if (sketchType == null)
                return null;

            var assetPath = GetPrefabAssetPath(sketchType);
            var assetName = GetPrefabAssetName(sketchType);
            var sketch = CreateSketch(sketchType, assetPath, assetName);
            return sketch;
        }

        /// <summary>
        /// Asset Bundle 중 일반적으로 Path 와 Name 으로 , 일단은 Resource 로.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="assetPath"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private UISketch CreateSketch(Type t, string assetPath, string assetName)
        {
            var sketch = _sketchPool.GetSketch(t);
            var isPool = sketch != null; // 현재 시점에서 Sketch 가 있다면 이는 Pool 오브젝트라는 점
            if (sketch == null)
            {
                var goObject = CreateObject(assetPath, assetName);
                if (goObject != null)
                    sketch = goObject.GetComponent<UISketch>();
            }

            if (sketch != null)
            {
                var tr = GetRoot(sketch.Layer);
                if (tr != null)
                {
                    sketch.transform.SetParentAndResetTransform(tr);
                    sketch.transform.SetAsLastSibling();
                }

                if (isPool)
                {
                    sketch.OnPoolingExit();
                }

            }

            return sketch;
        }

        // 추후 에셋 번들 / 혹은 리소스로 분기를 태워서 출력되겠슴 수정 적용되어야 합니다.
        // 현재는 ( 폴더도 리소스폴더내 분기화 하지 않은 ) 리소스 로드만 이용하기에 assetPath 는 이용하지 않습니다.
        private GameObject CreateObject(string assetPath, string assetName)
        {
            var loadObject = Resources.Load<GameObject>($"{assetName}");
            if (loadObject == null)
                return null;

            var goIns = UnityEngine.Object.Instantiate(loadObject, Vector3.zero, Quaternion.identity);
            if (goIns != null)
            {
                var goName = goIns.name;
                goIns.name = goName.Replace("(Clone)", string.Empty);
            }

            return goIns;
        }

        public void PushSketch(bool push, bool fullScreen, UISketch sketch)
        {
            var stack = GetStack(sketch.Layer);
            stack?.Add(push, sketch);

            if (fullScreen)
            {
                // TODO. full screen 대응 처리
                // 씬에 붙어 있는 ui 처리
                if (push)
                {
                }
                else
                {
                }
            }
        }

        /// <summary>
        /// 재활용이 되어야 하는 Sketch 를 이 함수를 통해 넣습니다.
        /// </summary>
        /// <param name="push"></param>
        /// <param name="sketch"></param>
        public void PushPoolSketch(UISketch sketch)
        {
            _sketchPool.Add(sketch);

            var poolRoot = GetRoot(UIRoot.Pool);
            if (poolRoot != null)
            {
                sketch.transform.SetParentAndResetTransform(poolRoot);
            }
        }

        public UISketchStack GetStack(UILayer layer)
        {
            if (_dicStack.ContainsKey(layer) == false)
            {
                _dicStack.Add(layer, new UISketchStack(layer, this));
            }

            return _dicStack[layer];
        }

        private Transform GetRoot(UIRoot root)
        {
            var go = _dicRoot.ContainsKey(root) ? _dicRoot[root] : null;
            return go != null ? go.transform : null;
        }

        private Transform GetRoot(UILayer layer)
        {
            switch (layer)
            {
                case UILayer.Content:
                    return GetRoot(UIRoot.Content);
                case UILayer.Popup:
                    return GetRoot(UIRoot.Popup);
                case UILayer.Toast:
                    return GetRoot(UIRoot.Toast);
            }

            return null;
        }
    }
}

