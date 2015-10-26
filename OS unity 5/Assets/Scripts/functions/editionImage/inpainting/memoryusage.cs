using UnityEngine;
using System.Collections;

public class memoryusage : MonoBehaviour {

    void OnGUI () {
        GUILayout.Label("All " + Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)).Length);
        GUILayout.Label("Textures " + Resources.FindObjectsOfTypeAll(typeof(Texture)).Length);
        GUILayout.Label("AudioClips " + Resources.FindObjectsOfTypeAll(typeof(AudioClip)).Length);
        GUILayout.Label("Meshes " + Resources.FindObjectsOfTypeAll(typeof(Mesh)).Length);
        GUILayout.Label("Materials " + Resources.FindObjectsOfTypeAll(typeof(Material)).Length);
        GUILayout.Label("GameObjects " + Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length);
        GUILayout.Label("Components " + Resources.FindObjectsOfTypeAll(typeof(Component)).Length);
    }
}