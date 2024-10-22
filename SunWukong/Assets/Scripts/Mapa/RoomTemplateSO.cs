using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Sala_", menuName = "Scriptable Objects/Grafos/Sala")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("ROOM PREFAB")]

    #endregion Header ROOM PREFAB

    public GameObject prefab;

    [HideInInspector] public GameObject previousPrefab;


    #region Configuração da sala

    [Space(10)]
    [Header("Configuração da sala")]

    #endregion Configuração da sala

    public RoomNodeTypeSO roomNodeType;

    //baixo esq
    public Vector2Int lowerBounds;

    //cima dir
    public Vector2Int upperBounds;

    //máximo de portas que o mapa pode ter
    [SerializeField] public List<Doorway> doorwayList;

    //possibilidade de spawn
    public Vector2Int[] spawnPositionArray;

    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation

#if UNITY_EDITOR

    // Validate SO fields
    private void OnValidate()
    {
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
    }

#endif

    #endregion Validation
}