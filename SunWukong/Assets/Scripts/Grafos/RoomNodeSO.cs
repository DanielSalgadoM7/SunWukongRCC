//as variáveis estarão em sua maioria em inglês para facilitar o entendimento do código, pq em português fica meio esquisito
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomNodeSO : ScriptableObject
{
    public string id;
    public List<string> idListaNodePai = new List<string>();
    public List<string> idListaNodeFilho = new List<string>();
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

        // Se o no tem pai ou é do tipo entrada define um label else exibe um popup
        if (idListaNodePai.Count > 0 || roomNodeType.isEntrada)
        {
            // da um label ao no que não pode ser alterado
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }else{

            //vai aparecer um popup com os possíveis nós
            int selected = roomNodeTypeList.lista.FindIndex(x => x == roomNodeType);

            //é a variável que vai armazenar o nó que foi selecionado
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.lista[selection];

            // If the room type selection has changed making child connections potentially invalid
            if (roomNodeTypeList.lista[selected].isCorredor && !roomNodeTypeList.lista[selection].isCorredor ||
                !roomNodeTypeList.lista[selected].isCorredor ||
                roomNodeTypeList.lista[selected].isSalaBoss && !roomNodeTypeList.lista[selection].isSalaBoss){
                if (idListaNodeFilho.Count > 0){
                    for (int i = idListaNodeFilho.Count - 1; i >= 0; i--){
                        // Get child room node
                        RoomNodeSO nodeFilho = roomNodeGraph.GetRoomNode(idListaNodeFilho[i]);

                        // If the child room node is not null
                        if (nodeFilho != null){
                            // Remove childID from parent room node
                            RemoveIdFilhoFromRoomNode(nodeFilho.id);

                            // Remove parentID from child room node
                            nodeFilho.RemoveIdPaiFromRoomNode(id);
                        }
                    }
                }
            }

        }

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

        //vai verificar se a sala já possui a quantidade de filhos máxima
        if (IsChildRoomValid(idFilho)){
            idListaNodeFilho.Add(idFilho);
            return true;
        }
        return false;
    }

    //vai verificar tudo que a gente pensou pra criação dos cenários
    public bool IsChildRoomValid(string idFilho) {
        bool isConnectedBossNode = false; // verifica se a sala já tem um caminho para a sala do boss, pra que não crie duas ramificações
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList) {
            if (roomNode.roomNodeType.isSalaBoss && roomNode.idListaNodePai.Count > 0) {
                isConnectedBossNode = true;
            }
        }

        //se já tem uma sala de boss conectada
        if (roomNodeGraph.GetRoomNode(idFilho).roomNodeType.isSalaBoss && isConnectedBossNode) {
            return false;
        }

        //se ele tem uma sala do tipo none
        if (roomNodeGraph.GetRoomNode(idFilho).roomNodeType.isNone) {
            return false;
        }

        //se já tem uma sala com esse id Filho ( não cria dois caminhos de um corredor só pra mesma sala
        if (idListaNodeFilho.Contains(idFilho)) {
            return false;
        }

        //não pode fazer um caminho pra própria sala né
        if (id == idFilho) {
            return false;
        }

        //se o novo nó já tá ligado no nó pai, não precisa de outro caminho
        if (idListaNodePai.Contains(idFilho)) {
            return false;
        }

        //se o nó já tem um nó pai ele retorna falso
        if (roomNodeGraph.GetRoomNode(idFilho).idListaNodePai.Count > 0) {
            return false;
        }

        //um corredor não pode ligar com outro corredor
        if (roomNodeGraph.GetRoomNode(idFilho).roomNodeType.isCorredor && roomNodeType.isCorredor) {
            return false;
        }

        //se a ultima sala não é um corredor, e o nó atual não é um corredor, não tem como ligar os dois
        if (!roomNodeGraph.GetRoomNode(idFilho).roomNodeType.isCorredor && !roomNodeType.isCorredor){
            return false;
        }

        // verifica se já tá com o max de filhos
        if (roomNodeGraph.GetRoomNode(idFilho).roomNodeType.isCorredor && idListaNodeFilho.Count >= Settings.maxCorredorFilho){
            return false;
        }

        // se o filho do nó é uma entrada, tá errado, pq a entrada é o que começa, não tem pai
        if (roomNodeGraph.GetRoomNode(idFilho).roomNodeType.isEntrada){
            return false;
        }
        // a gente vai criar um corredor que só conecta duas salas
        if (!roomNodeGraph.GetRoomNode(idFilho).roomNodeType.isCorredor && idListaNodeFilho.Count > 0){
                return false;
        }
        return true;

    }

    //vai adicionar os Id's pai para os nós
    public bool AddIdPaiToRoomNode(string idPai){
        idListaNodePai.Add(idPai);
        return true;
    }

    //remove nó filho
    public bool RemoveIdFilhoFromRoomNode(string idFilho) {
        // se o nó contem o id filho, remove ele
        if (idListaNodeFilho.Contains(idFilho))
        {
            idListaNodeFilho.Remove(idFilho);
            return true;
        }
        return false;
    }

    //remove nó pai
    public bool RemoveIdPaiFromRoomNode(string idPai)
    {
        // se o nó contem o id pai, remove ele
        if (idListaNodePai.Contains(idPai))
        {
            idListaNodePai.Remove(idPai);
            return true;
        }
        return false;
    }

#endif
    #endregion Editor Code
}