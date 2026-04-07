using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.InputSystem; // Add this using directive for EditorGUIUtility

[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public sealed class ObjectFinder : Singleton<ObjectFinder>
{

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying == false) 
            return;

        // 만약 마우스 휠을 클릭했을 때에
        if (Mouse.current.IsPressed(2))
        {
            // 현재 활성화된 SceneView를 가져옵니다.
            SceneView currentSceneView = SceneView.currentDrawingSceneView;

            // SceneView나 카메라가 유효하지 않으면 Raycast를 수행할 수 없습니다.
            if (currentSceneView == null || currentSceneView.camera == null)
            {
                // Debug.LogWarning("No active SceneView or camera found."); // Optional warning
                return;
            }

            // 마우스 위치를 기준으로 Ray를 생성합니다.
            // Input.mousePosition은 스크린 좌표계이므로 SceneView 카메라를 사용합니다.
            var ray = currentSceneView.camera.ScreenPointToRay(Input.mousePosition);

            // Raycast를 수행하여 가장 먼저 충돌하는 객체를 찾습니다.
            if (Physics.Raycast(ray, out var hit))
            {
                GameObject hitObject = hit.collider.gameObject;

                EditorGUIUtility.PingObject(hitObject);
                Debug.Log($"Pinged object: {hitObject.name}"); 
            }
            else
            {
                Debug.Log("No object hit by raycast."); 
            }
        }
    }
#endif
}
