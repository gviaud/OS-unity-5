
using UnityEngine;
using System.Collections;

/*[ExecuteInEditMode]*/
public class CameraFrustum : MonoBehaviour
{
	public float fovX;
    private bool m_active;

    //-----------------------------------------------------
    void Start()
    {
        fovX = transform.GetComponent<Camera>().fov;
        m_active = true;
    }
	
	//-----------------------------------------------------
    void LateUpdate()
    {
        if(!m_active) return;

		float width = Camera.main.pixelRect.width;//4.0f;//UnityEngine.Screen.width;
		float height = Camera.main.pixelRect.height;//3.0f;//UnityEngine.Screen.height;
		
		float frustumNear = transform.GetComponent<Camera>().nearClipPlane;
		float frustumFar = transform.GetComponent<Camera>().farClipPlane;
		
		float fovY;
		float aspect;
		float h;
		float w;
		
		if(width>height)
		{
			fovY = fovX * height / width;
			aspect = width / height;
		}
		else
		{
			fovY = fovX * width / height;
			aspect = width / height;
		}
		h = Mathf.Tan(fovY*Mathf.Deg2Rad) * frustumNear * 0.5f;
		w = h * aspect;
		Matrix4x4 m = PerspectiveOffCenter(-w, w, -h, h, frustumNear, frustumFar);
		GetComponent<Camera>().projectionMatrix = m;
		
	    //	Debug.Log("Cam Fov : "+camera.fieldOfView+", FovX : "+fovX);

	} // LateUpdate()

    //-----------------------------------------------------
    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far) {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }

    //-----------------------------------------------------
	public void SetFovX(float newFovX)
	{
		fovX = newFovX;
	}

    //-----------------------------------------------------
	public float GetFov()
	{
		return fovX;
	}

    //-----------------------------------------------------
    public void StopFrustum()
    {
        m_active = false;
        GetComponent<Camera>().ResetProjectionMatrix();
    }

    //-----------------------------------------------------
    public void StartFrustum()
    {
        m_active = true;
    }

} // class CameraFrustum
