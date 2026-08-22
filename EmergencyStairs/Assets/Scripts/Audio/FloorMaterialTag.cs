using UnityEngine;

/// <summary>床コライダーに付けて種類を示すだけのマーカー。FootstepControllerが下方向レイキャストで参照する。</summary>
public class FloorMaterialTag : MonoBehaviour
{
    public FloorMaterial material = FloorMaterial.Concrete;
}
