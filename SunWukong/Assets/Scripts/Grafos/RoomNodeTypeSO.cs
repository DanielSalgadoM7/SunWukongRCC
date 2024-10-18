//as variáveis estarão em sua maioria em inglês para facilitar o entendimento do código, pq em português fica meio esquisito
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NodeTipoSala_", menuName = "Scriptable Objects/Grafos/ Nó Tipo Sala")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    [Header("Marca qual sala que tá sendo alterada no editor")]
    public bool displayNodeEditorGrafos = true;

    [Header("Marcar corredores")]
    public bool isCorredor;

    [Header("Marcar corredores Norte Sul")]
    public bool isCorredorNS;

    [Header("Marcar corredores Leste Oeste")]
    public bool isCorredorLO;

    [Header("Marcar a entrada")]
    public bool isEntrada;

    [Header("Marcar a sala do boss")]
    public bool isSalaBoss;

    [Header("Marcar corredores")]
    public bool isNone;

    //usamos isso aqui pra testar só no editor, não vai pro jogo
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
}