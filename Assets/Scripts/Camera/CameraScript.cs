// This keeps the game view consistent regardless of device

using UnityEngine;

// https://gamedev.stackexchange.com/questions/167317/scale-camera-to-fit-screen-size-unity
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    // In-world distance between left & right edges of your scene.
    public float sceneWidth;
    // In-world distance between top & bottom edges of your scene.
    public float sceneHeight;

    public float sizeMod;

    private Camera _camera;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        float desiredHalfHeight;

        float unitsPerPixel = sceneWidth / Screen.width;
        desiredHalfHeight = sizeMod * unitsPerPixel * Screen.height;

        // Calculate size required to fit the scene height exactly
        float heightBasedHalfSize = sceneHeight / 2f;

        // Pick whichever is larger to ensure nothing is cut off
        _camera.orthographicSize = Mathf.Max(desiredHalfHeight, heightBasedHalfSize);
    }
}
