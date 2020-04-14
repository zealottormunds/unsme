using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshTypes : MonoBehaviour
{
    public enum TypeOfMesh
    {
        Naruto28,
        Naruto64,
        Jojo28,
        Jojo64
    };

    public string[] MeshName = new string[4]
    {
        "NARUTO, 28",
        "NARUTO, 64",
        "JOJO, 28",
        "JOJO, 64"
    };
}
