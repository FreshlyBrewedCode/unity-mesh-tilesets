using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MeshTilesetsEditor
{
    public class SceneOverlayWindow
    {
        public delegate void WindowFunction(SceneView sceneView);

        private object sceneOverlayWindow;
        private MethodInfo windowMethod;
        private object[] windowMethodParams;
        public UnityEngine.Object target;
    
        // public delegate void ShowWindowFunc(object overlayWindow);
        public delegate void OnWindowGUICallback(UnityEngine.Object target, SceneView sceneView);
        
        public SceneOverlayWindow(GUIContent title, OnWindowGUICallback onWindowGUI, UnityEngine.Object target, int priority = int.MaxValue)
        {
            this.target = target;
            var unityEditor = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
            
#if UNITY_2019_3 || UNITY_2019_4
            var overlayWindowType = unityEditor.GetType("UnityEditor.SceneViewOverlay+OverlayWindow");
#elif UNITY_2020_1_OR_NEWER
            var overlayWindowType = unityEditor.GetType("UnityEditor.OverlayWindow");
#endif
            var sceneViewOverlayType = unityEditor.GetType("UnityEditor.SceneViewOverlay");
            var windowFuncType = sceneViewOverlayType.GetNestedType("WindowFunction");
            var sceneViewFuncDelegate = Delegate.CreateDelegate(windowFuncType, onWindowGUI.Target, onWindowGUI.Method);

            var windowDisplayOptionType = sceneViewOverlayType.GetNestedType("WindowDisplayOption");
            var windowDisplayOption = Enum.Parse(windowDisplayOptionType, "OneWindowPerTarget");
            
#if UNITY_2019_3 || UNITY_2019_4
                windowMethod = sceneViewOverlayType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).FirstOrDefault(t => t.Name == "Window" && t.GetParameters().Length == 6);
#elif UNITY_2020_1_OR_NEWER  
                //public static void ShowWindow(OverlayWindow window)
                windowMethod = sceneViewOverlayType.GetMethod("ShowWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
#endif

#if UNITY_2019_3 || UNITY_2019_4
            windowMethodParams = new object[]
            { 
                title, sceneViewFuncDelegate, priority, target, windowDisplayOption, null
            };
#elif UNITY_2020_1_OR_NEWER
            //public OverlayWindow(GUIContent title, SceneViewOverlay.WindowFunction guiFunction, int primaryOrder, Object target, SceneViewOverlay.WindowDisplayOption option)
            sceneOverlayWindow = Activator.CreateInstance(overlayWindowType, 
                title,
                sceneViewFuncDelegate,
                int.MaxValue, this.target, 
                windowDisplayOption //SceneViewOverlay.WindowDisplayOption.OneWindowPerTarget
            );
            windowMethodParams = new object[] { sceneOverlayWindow };
#endif
            
            //showWindow = sceneViewOverlayType.GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.Public);
            // showWindow = Delegate.CreateDelegate(typeof(ShowWindowFunc), showSceneViewOverlay) as ShowWindowFunc;
        }

        public void ShowWindow()
        {
            if (windowMethod != null)
                windowMethod.Invoke(null, windowMethodParams);
        }

    }
}
