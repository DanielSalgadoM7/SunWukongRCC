//as variáveis estarão em sua maioria em inglês para facilitar o entendimento do código, pq em português fica meio esquisito
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Scriptable Objects/Grafos/Lista de Nós dos Tipos de Sala")]
public class RoomNodeTypeListSO : ScriptableObject{ 

    [Header("Room Node Type List")]

    [Tooltip("A lista vai ser preenchida com todos os tipos de nó dos objetos de script")]
    public List<RoomNodeTypeSO> lista;

#if  UNITY_EDITOR
    private void OnValidate(){
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(lista), lista);
    }
#endif
}