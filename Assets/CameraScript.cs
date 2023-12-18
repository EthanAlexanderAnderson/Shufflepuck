// This keeps the game view consistent regardless of device

using UnityEngine;

// https://gamedev.stackexchange.com/questions/167317/scale-camera-to-fit-screen-size-unity
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    // Set this to the in-world distance between the left & right edges of your scene.
    public float sceneWidth;

    Camera _camera;
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Adjust the camera's height so the desired scene width fits in view
    // even if the screen/window size changes dynamically.
    void Update()
    {
        float unitsPerPixel = sceneWidth / Screen.width;

        float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;

        _camera.orthographicSize = desiredHalfHeight;
    }
}
