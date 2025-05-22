using UnityEngine;


namespace Minimoo.Tools
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class CameraResolution : MonoBehaviour
    {
        public Camera targetCam;
        public Vector2 Landscape = new Vector2(640, 360);

        #region Unity Messages

        protected virtual void Start()
        {
            if (targetCam == null)
            {
                targetCam = GetComponent<Camera>();
            }

            UpdateResolution();
        }


        #endregion

        #region Public Methods
        public void UpdateResolution()
        {
            var rect = targetCam.rect;

            var targetSize = Landscape;

            var scalewidth = (Screen.height / (float)Screen.width) / (targetSize.y / (float)targetSize.x); // (세로 / 가로)
            var scaleheight = 1f / scalewidth;

            if (scalewidth < 1)
            {
                rect.width = scalewidth;
                rect.x = (1f - scalewidth) / 2f;
            }
            else
            {
                rect.height = scaleheight;
                rect.y = (1f - scaleheight) / 2f;
            }
            targetCam.rect = rect;
        }
        #endregion

    }
}
