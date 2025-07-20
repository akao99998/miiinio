using UnityEngine;

public class UVScroll : MonoBehaviour
{
	public int _uvTieX = 1;

	public int _uvTieY = 1;

	public int _seconds = 1;

	private int _fps;

	private Vector2 _size;

	private Renderer _myRenderer;

	private int _lastIndex = -1;

	private float time;

	private void Start()
	{
		_fps = _uvTieX * _uvTieY / _seconds;
		_size = new Vector2(1f / (float)_uvTieX, 1f / (float)_uvTieY);
		_myRenderer = GetComponent<Renderer>();
		if (_myRenderer == null)
		{
			base.enabled = false;
		}
		_myRenderer.material.SetTextureScale("_MainTex", _size);
		_myRenderer.material.SetTextureScale("_AlphaTex", _size);
		_myRenderer.material.SetTextureOffset("_MainTex", new Vector2(0f, 0.75f));
		_myRenderer.material.SetTextureOffset("_AlphaTex", new Vector2(0f, 0.75f));
	}

	private void Update()
	{
		time += Time.deltaTime;
		int num = (int)(time * (float)_fps) % (_uvTieX * _uvTieY);
		if (num != _lastIndex)
		{
			int num2 = num % _uvTieX;
			int num3 = num / _uvTieY;
			Vector2 offset = new Vector2((float)num2 * _size.x, 1f - _size.y - (float)num3 * _size.y);
			_myRenderer.material.SetTextureOffset("_MainTex", offset);
			_myRenderer.material.SetTextureOffset("_AlphaTex", offset);
			_lastIndex = num;
		}
	}
}
