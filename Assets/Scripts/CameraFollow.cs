using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    private Transform targetTransform;
    private Transform xform;
    private Vector3 offset = new Vector3(0f,3f,-2f);
    private Quaternion gameRotation = Quaternion.Euler(34f, 0f, 0f);

    private void FollowTarget()
    {
        xform.position = new Vector3(0f, 0f, targetTransform.position.z) + offset;
    }

    private void LoadGameCamera(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") { return; }

        xform = transform;   
        targetTransform = GameObject.FindGameObjectWithTag("Player").transform;

        xform.rotation = gameRotation;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += LoadGameCamera;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= LoadGameCamera;
    }

    private void Update()
    {
        if (!targetTransform) { return; }

        FollowTarget();
    }
}
