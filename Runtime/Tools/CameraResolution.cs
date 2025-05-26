using UnityEngine;


namespace Minimoo.Tools
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class CameraResolution : MonoBehaviour
    {
        public Camera targetCam;
        public Vector2 TargetResolution = new Vector2(640, 360);

        #region Unity Messages

        private void Awake()
        {
            if (targetCam == null)
            {
                targetCam = GetComponent<Camera>();
            }
        }

        void Start()
        {
            if (targetCam == null)
            {
                targetCam = GetComponent<Camera>();
            }

            UpdateResolution();
        }

        void Update()
        {
            UpdateResolution();
        }


        #endregion

        #region Public Methods
        public void UpdateResolution()
        {
            var rect = targetCam.rect;

            var targetSize = TargetResolution;

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
