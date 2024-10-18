//as variáveis estarão em sua maioria em inglês para facilitar o entendimento do código, pq em português fica meio esquisito
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
        //os nós de cada sala vão se instanciar sozinhos
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        //Carrega a lista dos nós do grafo criado
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    //vai desenhar cada nó com o estilo que a gente criou lá no layout
    //vê se coloca um laranja na cor de cada nó
    public void Draw(GUIStyle nodeStyle){
        //desenha a caixa do nó com a função do GUI
        GUILayout.BeginArea(rect, nodeStyle);

        //começa essa região pra ver as mudanças
        EditorGUI.BeginChangeCheck();
        
        //vai aparecer um popup com os possíveis nós
        int selected = roomNodeTypeList.lista.FindIndex(x => x == roomNodeType);

        //é a variável que vai armazenar o nó que foi selecionado
        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

        roomNodeType = roomNodeTypeList.lista[selection];

        //tudo que tive entre isso e o begin change vai ser salvo no popup
        if (EditorGUI.EndChangeCheck()){
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    //dentro do popup que a gente criou em cima, colocamos as opções de nó que podem aparecer nele
    public string[] GetRoomNodeTypesToDisplay(){
        string[] roomArray = new string[roomNodeTypeList.lista.Count];

        //itera sobre todos os nós
        for(int i = 0; i < roomNodeTypeList.lista.Count; i++){
            //se é o nome que a gente quer mostrar, ele mostra na string que vai ser retornada
            if (roomNodeTypeList.lista[i].displayNodeEditorGrafos){
                roomArray[i] = roomNodeTypeList.lista[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    //função que vai gerenciar as movimentações que podem acontecer com o nó dentro do editor
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
        //0 corresponde ao botão esquerdo e 1 ao botão direito
        if (currentEvent.button == 0) {
            ProcessLeftClickDownEvent();
        }else{
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessLeftClickDownEvent(){
        //com isso a gente vai conseguir arrastar todos os objetos que estão dentro do nó
        Selection.activeObject = this;

        // isso vai inverter o estado do botão esquerdo, como um toggle
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
        //0 corresponde ao botão esquerdo e 1 ao botão direito
        if (currentEvent.button == 0){
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent(){
        // quando o o botão esquerdo parar de ser pressionado, ele não vai mais arrastar o nó
        if (isLeftClickDragging){
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent){
        //0 corresponde ao botão esquerdo e 1 ao botão direito
        if (currentEvent.button == 0){
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent){
        // coloca a variavel pra true, mostrando que tá sendo arrastado, e chama a função que permite arrastar o nó
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta) {
        //move o nó de acordo com o valor de delta, no caso, os referenciais do editor
        rect.position += delta;
        //chama o SetDirty dnovo pra mostrar no assets database que precisa salvar
        EditorUtility.SetDirty(this);
    }

    //vai adicionar os Id's filho para os nós
    public bool AddIdFilhoToRoomNode(string idFilho){
        idListaNodeFilho.Add(idFilho);
        return true;
    }

    //vai adicionar os Id's pai para os nós
    public bool AddIdPaiToRoomNode(string idPai){
        idListaNodePai.Add(idPai);
        return true;
    }
#endif
    #endregion Editor Code
}