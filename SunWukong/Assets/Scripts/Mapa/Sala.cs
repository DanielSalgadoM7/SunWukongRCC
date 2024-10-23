using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sala
{
    public string id;
    public string templateID;
    public GameObject prefab;
    public RoomNodeTypeSO roomNodeType;
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;
    public Vector2Int[] spawnPositionArray;
    public List<string> listaIdSalaFilho;
    public string idSalaPai;
    public List<Doorway> doorWayList;
    public SalaInstanciada salaInstanciada;
    public bool isAcesso = false;
    public bool isSemInimigos = false;
    public bool isJaVisitado = false;

    public Sala()
    {
        listaIdSalaFilho = new List<string>();
        doorWayList = new List<Doorway>();
    }
}
