using UnityEngine;

/// <summary>
/// 注視時にエミッションカラーを発光させて対象を強調表示する汎用コンポーネント。
/// InteractableBase系オブジェクトに付けておくと、Interactorが自動でON/OFFする。
/// </summary>
public class Highlightable : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private float emissionIntensity = 0.6f;

    private MaterialPropertyBlock block;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetHighlighted(bool highlighted)
    {
        Color color = highlighted ? highlightColor * emissionIntensity : Color.black;
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(block);
            block.SetColor(EmissionColorId, color);
            r.SetPropertyBlock(block);
        }
    }
}
