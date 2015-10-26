using UnityEngine;
using System.Collections;

public class LowWallMapper : BaseMapper 
{
	public LowWallMapper () : base () {}
	
	public LowWallMapper(Mesh mesh) : base (mesh) {}
		
	public override void RecalculateTextureCoordinate ()
	{
		_mesh.RecalculateBounds ();
		Bounds b = _mesh.bounds;
		
		float rightPoint = b.extents.x;
		float forwardPoint = b.extents.z;
		float v = b.extents.y * 1.0f;
		
		float u = 0;
		Vector2 uv0 = new Vector2 (-u, 0);
		Vector2 uv1 = new Vector2 (-u, -v);
		u += rightPoint * 2;
		Vector2 uv2 = new Vector2 (-u, 0);
		Vector2 uv3 = new Vector2 (-u, -v);
		u += forwardPoint * 2;
		Vector2 uv4 = new Vector2 (-u, 0);
		Vector2 uv5 = new Vector2 (-u, -v);
		u += rightPoint * 2;
		Vector2 uv6 = new Vector2 (-u, 0);
		Vector2 uv7 = new Vector2 (-u, -v);
		u += forwardPoint * 2;
		Vector2 uv0b = new Vector2 (-u, 0);
		Vector2 uv1b = new Vector2 (-u, -v);
		
		_mesh.uv = new Vector2 [] {uv0, uv1, uv2, uv3,
								   uv2, uv3, uv4, uv5,
								   uv4, uv5, uv6, uv7,
								   uv6, uv7, uv0b, uv1b};
	}
}
