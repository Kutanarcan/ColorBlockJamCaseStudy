using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Moves the scene camera so the board fits the safe area minus the HUD margins (D73). Rotation and lens are
    /// the scene's (prototype setup); <see cref="CameraFit"/> decides the position.
    /// </summary>
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private FitMargins margins = new FitMargins(0.15f, 0.12f, 0.04f);

        public Camera SceneCamera => sceneCamera;

        public void Fit(Bounds board)
        {
            Rect viewport = CameraFit.Viewport(new Vector2(Screen.width, Screen.height), Screen.safeArea, margins);
            Transform view = sceneCamera.transform;
            view.position = CameraFit.Position(board, view.rotation, sceneCamera.fieldOfView, sceneCamera.aspect, viewport);
        }
    }
}
