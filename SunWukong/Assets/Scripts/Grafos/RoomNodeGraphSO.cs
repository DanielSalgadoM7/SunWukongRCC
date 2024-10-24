//as vari�veis estar�o em sua maioria em ingl�s para facilitar o entendimento do c�digo, pq em portugu�s fica meio esquisito

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Grafos/N� do Grafo das Salas")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDictionary();

    }

    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        //preenche a estrutura Dictionary, que armazena os n�s dos objetos
        foreach (RoomNodeSO node in roomNodeList)
        {
            roomNodeDictionary[node.id] = node;
        }
    }

    //pega um n� a partir do tipo do n�
    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType)
    {
        foreach (RoomNodeSO node in roomNodeList)
        {
            if (node.roomNodeType == roomNodeType)
            {
                return node;
            }
        }
        return null;
    }

    //pega um n� a partir do n� da sala
    public RoomNodeSO GetRoomNode(string roomNodeID)
    {
        if (roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }

    /// pega salas filho para suprir salas pai que ainda n�o tem todas as salas preenchidas
    public IEnumerable<RoomNodeSO> GetNodeSalaFilho(RoomNodeSO nodeSalaPai)
    {
        foreach (string idNodeFilho in nodeSalaPai.idListaNodeFilho)
        {
            yield return GetRoomNode(idNodeFilho);
        }
    }


    #region Editor Code
#if UNITY_EDITOR

    //pegar de qual n� a vari�vel vai come�ar
    [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 linePosition;

    //vai preencher a estrutura de dados dnovo toda vez que tiver alguma mudan�a no editor
    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }

#endif
    #endregion Editor Code

}
