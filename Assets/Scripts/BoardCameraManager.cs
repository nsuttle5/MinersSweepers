using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardCameraManager : MonoBehaviour
{
    [Header("Movement Stuff")]
    [SerializeField] private Camera viewportCam;
    [SerializeField] private float translationPanSpeed = 1.0f;
    [SerializeField] private float dragDeadzonePixelThreshold = 12f;

    [Header("Constraint Stuff")]
    [SerializeField] private EdgeClampStrategy borderStrategy = EdgeClampStrategy.MatchGridWithPaddingBuffer;
    [SerializeField] private float mapPaddingOffsetUnits = 3.5f;

    [Header("Inputs")]
    [SerializeField] private InputAction resetCameraAction = new(
        type: InputActionType.Button,
        binding: "<Keyboard>/r");

    private Vector3 screenSpaceInteractionAnchor;
    private Vector3 cameraInitialWorldPosition;
    private float incrementalDragDistanceTracked = 0f;
    private float lockedZPosition;

    public static bool IsDraggingCamera { get; private set; } = false;


    private void OnEnable()
    {
        resetCameraAction.performed += OnResetCameraPerformed;
        resetCameraAction.Enable();
    }

    private void OnDisable()
    {
        resetCameraAction.performed -= OnResetCameraPerformed;
        resetCameraAction.Disable();
    }

    private void Start()
    {
        if (!viewportCam) viewportCam = GetComponent<Camera>();
        lockedZPosition = transform.position.z;
    }

    private void Update()
    {
        ManageCameraTranslationLoop();
    }

    private void ManageCameraTranslationLoop()
    {
        Mouse currentMouse = Mouse.current;
        if (currentMouse == null) return;

        Vector2 mousePosition = currentMouse.position.ReadValue();

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            screenSpaceInteractionAnchor = new Vector3(mousePosition.x, mousePosition.y, viewportCam.nearClipPlane);
            cameraInitialWorldPosition = transform.position;
            IsDraggingCamera = false;
            incrementalDragDistanceTracked = 0f;
            return;
        }

        if (currentMouse.leftButton.isPressed)
        {
            Vector3 currentMouseCoordinates = new(mousePosition.x, mousePosition.y, viewportCam.nearClipPlane);

            Vector3 interactionVector = currentMouseCoordinates - screenSpaceInteractionAnchor;
            incrementalDragDistanceTracked = interactionVector.magnitude;

            if (incrementalDragDistanceTracked > dragDeadzonePixelThreshold)
            {
                IsDraggingCamera = true;
            }

            if (IsDraggingCamera)
            {
                Vector3 worldAnchorStart = viewportCam.ScreenToWorldPoint(screenSpaceInteractionAnchor);
                Vector3 worldAnchorCurrent = viewportCam.ScreenToWorldPoint(currentMouseCoordinates);

                Vector3 panningWorldDelta = worldAnchorStart - worldAnchorCurrent;
                panningWorldDelta.z = 0f;

                Vector3 estimatedPosition = cameraInitialWorldPosition + (panningWorldDelta * translationPanSpeed);
                estimatedPosition.z = lockedZPosition;

                transform.position = ConstrainCameraToBorders(estimatedPosition);
            }
        }

        if (currentMouse.leftButton.wasReleasedThisFrame)
        {
            StartCoroutine(SettleCameraStateFrameEnd());
        }
    }

    private Vector3 ConstrainCameraToBorders(Vector3 projectedCoords)
    {
        if (borderStrategy == EdgeClampStrategy.InfiniteBounds || BoardManager.Instance == null) return projectedCoords;

        float floorX = 0f;
        float ceilingX = BoardManager.Instance.Width;
        float floorY = -BoardManager.Instance.Height;
        float ceilingY = 0f;

        if (borderStrategy == EdgeClampStrategy.MatchGridWithPaddingBuffer)
        {
            ceilingX += mapPaddingOffsetUnits;
            floorY -= mapPaddingOffsetUnits;
        }

        projectedCoords.x = Mathf.Clamp(projectedCoords.x, floorX, ceilingX);
        projectedCoords.y = Mathf.Clamp(projectedCoords.y, floorY, ceilingY);
        projectedCoords.z = lockedZPosition;

        return projectedCoords;
    }

    private IEnumerator SettleCameraStateFrameEnd()
    {
        yield return new WaitForEndOfFrame();
        IsDraggingCamera = false;
    }

    private void OnResetCameraPerformed(InputAction.CallbackContext context)
    {
        transform.position = new(0, 0, lockedZPosition);
    }
}

public enum EdgeClampStrategy { InfiniteBounds, MatchGridExact, MatchGridWithPaddingBuffer }