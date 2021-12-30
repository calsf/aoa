using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// For WorldSpace canvas, this will make selected UI elements show on top of everything
public class WorldSpaceToOverlay : MonoBehaviour
{
    private const string shaderTestMode = "unity_GUIZTestMode";
    UnityEngine.Rendering.CompareFunction UICompareFunction = UnityEngine.Rendering.CompareFunction.Always;
    [SerializeField] Graphic[] UIElements;

    private Dictionary<Material, Material> materialMappings = new Dictionary<Material, Material>();

    protected virtual void Start()
    {
        foreach (Graphic graphic in UIElements)
        {
            Material material = graphic.materialForRendering;
            if (material == null)
            {
                continue;
            }
            if (!materialMappings.TryGetValue(material, out Material materialCopy))
            {
                materialCopy = new Material(material);
                materialMappings.Add(material, materialCopy);
            }
            materialCopy.SetInt(shaderTestMode, (int)UICompareFunction);
            graphic.material = materialCopy;
        }
    }
}
