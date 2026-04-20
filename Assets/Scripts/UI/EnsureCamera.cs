using UnityEngine;

namespace TicTacToe.UI
{
    public sealed class EnsureCamera : MonoBehaviour
    {
        [SerializeField] private Color background = new Color(0.06f, 0.06f, 0.07f, 1f);

        private void Awake()
        {
            if (Camera.allCamerasCount > 0)
                return;

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
        }
    }
}

