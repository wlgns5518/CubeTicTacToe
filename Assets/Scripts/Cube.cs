using UnityEngine;

public class Cube : MonoBehaviour
{
    public MeshRenderer cubeMesh;
    // 자식 오브젝트에서 "O"와 "X"를 찾아 활성화
    public Transform oObj;
    public Transform xObj;
    private void Awake()
    {
        cubeMesh = this.GetComponent<MeshRenderer>();
    }
}
