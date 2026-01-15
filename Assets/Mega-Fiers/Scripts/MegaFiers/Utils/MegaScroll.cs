using UnityEngine;

[ExecuteInEditMode]
public class MegaScroll : MonoBehaviour
{
	public float pos = 0.0f;
	public float gap = 0.5f;
	public Vector3 wpos;
	MegaBend[] bends;

	private void Start()
	{
		bends = GetComponents<MegaBend>();

		if ( bends.Length == 0 )
		{
			Mesh mesh = MegaUtils.GetSharedMesh(gameObject);

			MegaModifyObject modobj = gameObject.AddComponent<MegaModifyObject>();
			modobj.UpdateMode = MegaUpdateMode.LateUpdate;

			Bounds box = mesh.bounds;

			MegaBend bend1 = gameObject.AddComponent<MegaBend>();
			bend1.angle = -2500.0f;
			bend1.doRegion = true;
			bend1.from = -5.0f;
			bend1.gizmoPos = new Vector3(-box.size.x * 0.5f, 0.0f, 0.0f);
			bend1.gizmoRot = new Vector3(0.0f, -0.5f, -0.5f);
			bend1.Offset = new Vector3(0.0f, 0.0f, 0.0f);

			MegaBend bend2 = gameObject.AddComponent<MegaBend>();
			bend2.angle = -2500.0f;
			bend2.doRegion = true;
			bend2.to = 5.0f;
			bend2.gizmoPos = new Vector3(box.size.x * 0.5f, 0.0f, 0.0f);
			bend2.gizmoRot = new Vector3(0.0f, 1.0f, 1.5f);
			bend2.Offset = new Vector3(-box.size.x * 0.5f, 0.0f, 0.0f);

			pos = 0.0f;
			gap = -box.size.x * 0.5f;
		}
	}

	void Update()
	{
		if ( bends.Length >= 2 )
		{
			bends[1].gizmoPos.x = pos - gap;
			bends[0].gizmoPos.x = pos + gap;

			Vector3 p = transform.localPosition;

			p.x = wpos.x + pos;
			transform.localPosition = p;
		}
	}
}
