//as vari�veis estar�o em sua maioria em ingl�s para facilitar o entendimento do c�digo, pq em portugu�s fica meio esquisito
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Scriptable Objects/Grafos/Lista de N�s dos Tipos de Sala")]
public class RoomNodeTypeListSO : ScriptableObject{ 

    [Header("Room Node Type List")]

    [Tooltip("A lista vai ser preenchida com todos os tipos de n� dos objetos de script")]
    public List<RoomNodeTypeSO> lista;

#if  UNITY_EDITOR
    private void OnValidate(){
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(lista), lista);
    }
#endif
}