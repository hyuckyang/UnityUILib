using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace UILib
{
    public class UISketch : MonoBehaviour
    {
        public static string AssetPath => string.Empty;

        private enum UIEvent
        {
            Awake,
            Start,
            Destroy,
            Enable,
            Disable,
            Update,
            LateUpdate,

            Initialize,
            Release
        }

        private enum UIRender
        {
            Disable, // 화면에서 그려주지 않음
            Wait, // 대기
            Render, // 실제 화면에 그려줌
            Pool, // Pool 으로 들어가 있음
        }

        // 활성화 직후, 최소 2 프레임을 대기 합니다.
        private const int RenderWaitFrame = 2;
        private int _renderWaitFrame = 0;
        private UIRender _render = UIRender.Disable;

        public virtual UILayer Layer => UILayer.Content;
        protected virtual bool IsPooling => false;
        protected virtual bool IsFullScreen => true;
        protected UIManager Manager => UIManager.Instance;

        #region abstract

        // Unity Schedule
        protected virtual void OnSketchAwake()
        {
        }

        protected virtual void OnSketchStart()
        {
        }

        protected virtual void OnSketchDestroy()
        {
        }

        protected virtual void OnSketchEnable()
        {
        }

        protected virtual void OnSketchDisable()
        {
        }

        protected virtual void OnSketchUpdate()
        {
        }

        protected virtual void OnSketchLateUpdate()
        {
        }

        // 아래는 Custom Schedule
        /// <summary>
        /// Render 직전 1회 호출
        /// </summary>
        protected virtual void OnSketchInitialize()
        {
        }

        /// <summary>
        /// Disable( Destroy or pooling ) 직전 1회 호출
        /// </summary>
        protected virtual void OnSketchRelease()
        {
        }

        /// <summary>
        /// 뎁스가 높은 UI 가 올라오거나 사라질 때 1회 호출
        /// </summary>
        /// <param name="overWrite"> 현재의 Sketch 가 또다른 sketch 에 의해 덮어씌워지는 것인지 아니면 되려 해체 되는 것인지 </param>
        /// <param name="fullScreen">현재의 Sketch 가 덮어씌워지거는 혹은 해체 될 때 그 영향을 주는 Sketch 가 full screen 인지 아닌지 </param>
        protected virtual void OnSketchOverWrite(bool overWrite, bool fullScreen)
        {
        }

        #endregion

        private void Awake()
        {
            // 기본적으로 처음에는 렌더링을 비활성화 합니다.
            // 최소 2 프레임 내에 다시 활성화 됩니다.
            // pool 이 되는 sketch 는 pool 에 들어갈때 기본적으로 비활성화 되기에 따로 처리할 필요가 없습니다.
            RenderSketch(false);
            InvokeEvent(UIEvent.Awake);
        }

        private void Start()
        {
            InvokeEvent(UIEvent.Start);
        }

        private void OnDestroy()
        {
            InvokeEvent(UIEvent.Destroy);
        }

        private void OnEnable()
        {
            InvokeEvent(UIEvent.Enable);
        }

        private void OnDisable()
        {
            InvokeEvent(UIEvent.Disable);
        }

        private void Update()
        {
            if (IsPooling)
            {
                if (_render == UIRender.Pool)
                    return;
            }

            InvokeEvent(UIEvent.Update);
        }

        private void LateUpdate()
        {
            if (IsPooling)
            {
                if (_render == UIRender.Pool)
                    return;
            }

            if (_render == UIRender.Disable)
            {
                _render = UIRender.Wait;
                _renderWaitFrame = 0;
            }

            if (_render == UIRender.Wait)
            {
                _renderWaitFrame++;
                if (_renderWaitFrame == 1)
                {
                    // Scene Render
                }
                else
                {
                    if (_renderWaitFrame >= RenderWaitFrame)
                    {
                        OnInitialized();
                        RenderSketch(true);
                        OverWriteSketch(true);

                        _render = UIRender.Render;
                    }
                }
            }

            InvokeEvent(UIEvent.LateUpdate);
        }

        private void OnInitialized()
        {
            OnPushSketch(true);
            InvokeEvent(UIEvent.Initialize);
        }

        private void OnRelease()
        {
            InvokeEvent(UIEvent.Release);
        }

        /// <summary>
        /// Sketch 가 UI 로 올라갈 때 그 아래 깔리는 Sketeh 에 해당 정보는 넘겨주면
        /// 그려줄지 비활성화 할지를 분기
        /// </summary>
        /// <param name="over"></param>
        private void OverWriteSketch(bool over)
        {
            var prev = PrevSketch();
            if (prev == null)
                return;

            if (over)
            {
                // prev sketch 가 스스로
                prev.OnSketchOverWrite(true, IsFullScreen);
                if (IsFullScreen)
                {
                    // TODO 바로 이전 Sketch 의 랜더링만 활성화 여부를 결정하다보니 , 뎁스가 쌓일 수록 제대로 대응이 안된다. 
                    prev.RenderSketch(false);
                    prev.OnSketchDisable();
                }
            }
            else
            {
                if (IsFullScreen)
                {
                    // TODO 바로 이전 Sketch 의 랜더링만 활성화 여부를 결정하다보니 , 뎁스가 쌓일 수록 제대로 대응이 안된다.
                    prev.RenderSketch(true);
                    prev.OnSketchEnable();
                }

                prev.OnSketchOverWrite(false, IsFullScreen);
            }
        }

        public void OnClose()
        {
            OnCloseSketch();
        }

        public void OnCloseSketch()
        {
            OverWriteSketch(false);
            OnPushSketch(false);
            // TODO. Scene 처리
            // SceneUI Enable

            // Apply Tween 처리
            DestroyOrPooling();
        }

        private void OnPushSketch(bool push)
        {
            Manager?.PushSketch(push, IsFullScreen, this);
        }

        // Pool 에 나오는 단계
        // 즉 pool 에 나와서 활성화 됨
        public void OnPoolingExit()
        {
            if (!IsPooling)
                return;

            _render = UIRender.Disable;
        }

        private void OnPoolingEnter()
        {
            if (!IsPooling)
                return;

            // pool 공간에 들어가기에 비활성화가 되어야 합니다.
            RenderSketch(false);
            // manager 의 풀링 공간에 등록
            Manager?.PushPoolSketch(this);
            _render = UIRender.Pool;
        }

        private void DestroyOrPooling()
        {
            OnRelease();
            if (IsPooling)
            {
                OnPoolingEnter();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InvokeEvent(UIEvent uiEvent)
        {
            switch (uiEvent)
            {
                case UIEvent.Awake:
                    OnSketchAwake();
                    break;
                case UIEvent.Start:
                    OnSketchStart();
                    break;
                case UIEvent.Destroy:
                    OnSketchDestroy();
                    break;
                case UIEvent.Enable:
                    OnSketchEnable();
                    break;
                case UIEvent.Disable:
                    OnSketchDisable();
                    break;
                case UIEvent.Update:
                    OnSketchUpdate();
                    break;
                case UIEvent.LateUpdate:
                    OnSketchLateUpdate();
                    break;
                case UIEvent.Initialize:
                    OnSketchInitialize();
                    break;
                case UIEvent.Release:
                    OnSketchRelease();
                    break;
            }
        }

        /// <summary>
        /// Sketch 는 기본적으로 객체를 비활성화 하지 않고 최상단 Canvas 구성요소만 비활성화합니다.
        /// </summary>
        /// <param name="render"></param>
        private void RenderSketch(bool render)
        {
            // 
            gameObject.ForeachFindChildComponent<Canvas>(canvas =>
            {
                // CanvasScaler 를 통해 현재 캔버스가 ( 병렬일 수도 있으나 ) 최상단 캔버스임을 확인 할 수 있습니다. 
                if (canvas.GetComponent<CanvasScaler>() == null)
                    return;

                canvas.enabled = render;
                var raycast = canvas.GetComponent<GraphicRaycaster>();
                if (raycast != null)
                    raycast.enabled = render;

                // 카메라를 참조는 canvas 라면 해당 카메라를 활성화/비활성화 합니다.
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay &&
                    canvas.worldCamera != null)
                    canvas.worldCamera.enabled = enabled;
            });
        }

        /// <summary>
        /// Depth 는 하이라키 순서대로 강제로 뎁스를 설정합니다.
        /// 하이라키 순서대로 뎁스를 강제하지 않으면 추후 너무 중구난방으로 뎁스가 꼬입니다.
        /// </summary>
        /// <param name="push"></param>
        /// <param name="increase"></param>
        /// <returns></returns>
        public int SortSketch(bool push, int increase)
        {
            var depthCount = 0;
            gameObject.ForeachFindChildComponent<Canvas>(canvas =>
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    depthCount++;
                    if (push)
                        canvas.sortingOrder = depthCount + increase;
                }
            });

            gameObject.ForeachFindChildComponent<Camera>(cam =>
            {
                depthCount++;
                if (push)
                    cam.depth = depthCount + increase;
            });
            return depthCount;
        }

        private UISketch PrevSketch()
        {
            UISketch prevSketch = null;
            // Sketch 가 속한 Stack 의 Count 가 1 이상이라면 자신 이외에 다른 Sketch 있다는 것을 의미 합니다.
            // ( 이전 인지 이후 일지는 아래 로직을 통해 구분 됨)
            var stack = Manager?.GetStack(Layer);
            if (stack != null && stack.StackCount > 1)
            {
                var idx = stack.IndexOf(this);
                if (idx > 0)
                {
                    prevSketch = stack.FindSketch(idx - 1);
                }
            }

            var prevLayer = Layer - 1;
            var prevStack = Manager?.GetStack(prevLayer);
            var prevStackLastedSketch = prevStack?.LastedSketch;
            if (prevSketch == null && prevStackLastedSketch != null)
            {
                prevSketch = prevStackLastedSketch;
            }

            return prevSketch;
        }
    }
}