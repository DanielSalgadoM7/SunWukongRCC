//as variáveis estarão em sua maioria em inglês para facilitar o entendimento do código, pq em português fica meio esquisito
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if(instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    [Header ("Dungeon")]

    [Tooltip("Vai preencher com a RoomNodeTypeListSO ")]
    public RoomNodeTypeListSO roomNodeTypeList;
}
