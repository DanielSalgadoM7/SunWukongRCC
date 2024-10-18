//as vari�veis estar�o em sua maioria em ingl�s para facilitar o entendimento do c�digo, pq em portugu�s fica meio esquisito
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> idListaNodePai = new List<string>();
    [HideInInspector] public List<string> idListaNodeFilho = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    //get e set pra inicar os grafos no Editor
    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType) {
        //os n�s de cada sala v�o se instanciar sozinhos
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        //Carrega a lista dos n�s do grafo criado
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    //vai desenhar cada n� com o estilo que a gente criou l� no layout
    //v� se coloca um laranja na cor de cada n�
    public void Draw(GUIStyle nodeStyle){
        //desenha a caixa do n� com a fun��o do GUI
        GUILayout.BeginArea(rect, nodeStyle);

        //come�a essa regi�o pra ver as mudan�as
        EditorGUI.BeginChangeCheck();
        
        //vai aparecer um popup com os poss�veis n�s
        int selected = roomNodeTypeList.lista.FindIndex(x => x == roomNodeType);

        //� a vari�vel que vai armazenar o n� que foi selecionado
        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

        roomNodeType = roomNodeTypeList.lista[selection];

        //tudo que tive entre isso e o begin change vai ser salvo no popup
        if (EditorGUI.EndChangeCheck()){
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    //dentro do popup que a gente criou em cima, colocamos as op��es de n� que podem aparecer nele
    public string[] GetRoomNodeTypesToDisplay(){
        string[] roomArray = new string[roomNodeTypeList.lista.Count];

        //itera sobre todos os n�s
        for(int i = 0; i < roomNodeTypeList.lista.Count; i++){
            //se � o nome que a gente quer mostrar, ele mostra na string que vai ser retornada
            if (roomNodeTypeList.lista[i].displayNodeEditorGrafos){
                roomArray[i] = roomNodeTypeList.lista[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    //fun��o que vai gerenciar as movimenta��es que podem acontecer com o n� dentro do editor
    public void ProcessEvents(Event currentEvent) {
        switch (currentEvent.type) {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    //clica no mouse
    private void ProcessMouseDownEvent(Event currentEvent) {
        //0 corresponde ao bot�o esquerdo e 1 ao bot�o direito
        if (currentEvent.button == 0) {
            ProcessLeftClickDownEvent();
        }else{
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessLeftClickDownEvent(){
        //com isso a gente vai conseguir arrastar todos os objetos que est�o dentro do n�
        Selection.activeObject = this;

        // isso vai inverter o estado do bot�o esquerdo, como um toggle
        if (isSelected == true){
            isSelected = false;
        }else{
            isSelected = true;
        }
    }

    private void ProcessRightClickDownEvent(Event currentEvent){
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessMouseUpEvent(Event currentEvent){
        //0 corresponde ao bot�o esquerdo e 1 ao bot�o direito
        if (currentEvent.button == 0){
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent(){
        // quando o o bot�o esquerdo parar de ser pressionado, ele n�o vai mais arrastar o n�
        if (isLeftClickDragging){
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent){
        //0 corresponde ao bot�o esquerdo e 1 ao bot�o direito
        if (currentEvent.button == 0){
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent){
        // coloca a variavel pra true, mostrando que t� sendo arrastado, e chama a fun��o que permite arrastar o n�
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta) {
        //move o n� de acordo com o valor de delta, no caso, os referenciais do editor
        rect.position += delta;
        //chama o SetDirty dnovo pra mostrar no assets database que precisa salvar
        EditorUtility.SetDirty(this);
    }

    //vai adicionar os Id's filho para os n�s
    public bool AddIdFilhoToRoomNode(string idFilho){
        idListaNodeFilho.Add(idFilho);
        return true;
    }

    //vai adicionar os Id's pai para os n�s
    public bool AddIdPaiToRoomNode(string idPai){
        idListaNodePai.Add(idPai);
        return true;
    }
#endif
    #endregion Editor Code
}