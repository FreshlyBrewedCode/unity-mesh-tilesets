using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MeshTilesetsEditor
{
    public class SceneOverlayWindow
    {
        private object sceneOverlayWindow;
        private MethodInfo showWindow;
        public UnityEngine.Object target;
    
        // public delegate void ShowWindowFunc(object overlayWindow);
        public delegate void OnWindowGUICallback(UnityEngine.Object target, SceneView sceneView);
    
        public SceneOverlayWindow(GUIContent title, OnWindowGUICallback onWindowGUI, UnityEngine.Object target, int priority = int.MaxValue)
        {
            this.target = target;
            var unityEditor = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
            var overlayWindowType = unityEditor.GetType("UnityEditor.OverlayWindow");
            var sceneViewOverlayType = unityEditor.GetType("UnityEditor.SceneViewOverlay");
            var windowFuncType = sceneViewOverlayType.GetNestedType("WindowFunction");
            var windowFunc = Delegate.CreateDelegate(windowFuncType, onWindowGUI.Target, onWindowGUI.Method);
            var windowDisplayOptionType = sceneViewOverlayType.GetNestedType("WindowDisplayOption");
            sceneOverlayWindow = Activator.CreateInstance(overlayWindowType, 
                title,
                windowFunc,
                int.MaxValue, this.target, 
                Enum.Parse(windowDisplayOptionType, "OneWindowPerTarget") //SceneViewOverlay.WindowDisplayOption.OneWindowPerTarget
            );
            showWindow = sceneViewOverlayType.GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.Public);

            // showWindow = Delegate.CreateDelegate(typeof(ShowWindowFunc), showSceneViewOverlay) as ShowWindowFunc;
        }

        public void ShowWindow()
        {
            this.showWindow.Invoke(null, new object[]{sceneOverlayWindow});
        }

    }
}
