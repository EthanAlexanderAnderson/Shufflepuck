// This keeps the game view consistent regardless of device

using UnityEngine;

// https://gamedev.stackexchange.com/questions/167317/scale-camera-to-fit-screen-size-unity
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    // Set this to the in-world distance between the left & right edges of your scene.
    public float sceneWidth;
    public float ratioCutoff;
    public float sizeMod;
    public float ratioCutoff2;
    public float sizeMod2;
    public float sizeMod3;

    Camera _camera;
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Adjust the camera's height so the desired scene width fits in view
    // even if the screen/window size changes dynamically.
    void Update()
    {
        if (!(Screen.width < Screen.height * ratioCutoff))
        {
            float unitsPerPixel = sceneWidth / Screen.width;

            float desiredHalfHeight = sizeMod * unitsPerPixel * Screen.height;

            _camera.orthographicSize = desiredHalfHeight;
        }
        else if (!(Screen.width < Screen.height * ratioCutoff2))
        {
            float unitsPerPixel = sceneWidth / Screen.width;

            float desiredHalfHeight = sizeMod2 * unitsPerPixel * Screen.height;

            _camera.orthographicSize = desiredHalfHeight;
        }
        else
        {
            float unitsPerPixel = sceneWidth / Screen.width;

            float desiredHalfHeight = sizeMod3 * unitsPerPixel * Screen.height;

            _camera.orthographicSize = desiredHalfHeight;
        }
    }
}
