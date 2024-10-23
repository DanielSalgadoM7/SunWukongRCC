using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NivelDoMapa_", menuName ="Scriptable Objects/Grafos/Nivel do Mapa")]
public class NivelMapaSO : ScriptableObject
{
    #region Informações básicas
    [Header("Informações básicas sobre o nível")]
    #endregion Informações básicas
    public string nomeNivel;

    #region Sala para cada nível
    [Header("Salas para cada nível criado")]
    #endregion Sala para cada nível
    public List<RoomTemplateSO> roomTemplateList;

    #region Nós de cada sala
    [Header("Nós de cada sala por nível")]
    #endregion Nós de cada sala
    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(nomeNivel), nomeNivel);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;


        bool isCorredorLO = false;
        bool isCorredorNS = false;
        bool isEntrada = false;

        foreach(RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
                return;

            if (roomTemplateSO.roomNodeType.isCorredorLO)
                isCorredorLO = true;

            if (roomTemplateSO.roomNodeType.isCorredorNS)
                isCorredorNS = true;

            if (roomTemplateSO.roomNodeType.isEntrada)
                isEntrada = true;
        }

        if(isCorredorLO == false)
        {
            Debug.Log("No " + this.name.ToString() + " : Nenhum tipo de sala de Corredor Leste Oeste especificado");
        }

        if (isCorredorNS == false)
        {
            Debug.Log("No " + this.name.ToString() + " : Nenhum tipo de sala de Corredor Norte Sul especificado");
        }

        if (isEntrada == false)
        {
            Debug.Log("No " + this.name.ToString() + " : Nenhum tipo de sala de entrada especificado");
        }

        //percorre todos os nós do grafo
        foreach(RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
                return;

            //percore todos os nós do grafo selecionado
            foreach (RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if (roomNodeSO == null)
                    continue;

                if (roomNodeSO.roomNodeType.isEntrada || roomNodeSO.roomNodeType.isCorredorLO || roomNodeSO.roomNodeType.isCorredorNS || roomNodeSO.roomNodeType.isCorredor || roomNodeSO.roomNodeType.isNone)
                    continue;

                bool isNodeTipoSalaEncontrada = false;

                //percorre todas as salas do template pra ver se o nó foi especificado
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null)
                        continue;

                    if(roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isNodeTipoSalaEncontrada = true;
                        break;
                    }
                }

                if (!isNodeTipoSalaEncontrada)
                    Debug.Log("No " + this.name.ToString() + " : Sem modelo de sala " + roomNodeSO.roomNodeType.name.ToString() + " encontrado para os nós dos grafos " + roomNodeGraph.name.ToString());
            }
        }

    }
#endif 
    #endregion Validation
}
