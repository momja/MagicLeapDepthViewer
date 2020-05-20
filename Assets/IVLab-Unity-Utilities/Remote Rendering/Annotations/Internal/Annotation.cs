using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Annotation
{
	public Annotation(Vector3 position, Color color, float size, bool active)
	{
		x = position.x;
		y = position.y;
		z = position.z;
		r = color.r;
		g = color.g;
		b = color.b;
		this.size = size;
		this.active = active;
	}
	public Color Color { get { return new Color(r, g, b); } set { r = value.r; g = value.g; b = value.b; } }
	public Vector3 Position { get { return new Vector3(x, y, z); } set { x = value.x; y = value.y; z = value.z; } }

	[SerializeField] public float x;
	[SerializeField] public float y;
	[SerializeField] public float z;
	[SerializeField] public float r;
	[SerializeField] public float g;
	[SerializeField] public float b;
	[SerializeField] public float size;
	[SerializeField] public bool active;

}
public enum AnnotationMessageTypes
{
	UNKNOWN,
	ESTABLISH,
	CLOSE,
	UPDATE
}
