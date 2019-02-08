using UnityEngine;

public class Brush : FurInteractable
{
    [SerializeField]
    private Color _color;
    public Color color {  get { return _color; } }

    [SerializeField]
    private MeshRenderer _brushRenderer;

    [SerializeField]
    private int _brushColorMaterialIndex;

    public void Awake()
    {
        _brushRenderer.materials[_brushColorMaterialIndex].color = _color;
    }

    public override void Interact()
    {

    }
}
