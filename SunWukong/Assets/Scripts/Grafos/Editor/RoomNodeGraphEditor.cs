//as variáveis estarão em sua maioria em inglês para facilitar o entendimento do código, pq em português fica meio esquisito

using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    //valor de cada nó
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    //valor de cada linha que conecta os nós
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;

    [MenuItem("Editor de Grafos", menuItem = "Window/Grafos/EditorGrafosSalas")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Editor do Grafo das Salas");
    }

    //permite que a gente consiga desenhar os grafos na nova janela do projeto
    private void OnEnable()
    {
        // Adiciona ao inspector o envento de mudança de tela para o editor do RoomNodeGraph
        Selection.selectionChanged += InspectorSelectionChanged;

        //isso aqui é css dos botões que organizam o grafo das salas
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // "css" dos botões que foram selecionados
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor= Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);



        //Preenche o grafo com os tipos de sala
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    //abre a janela do projeto se clicar no inspetor do objeto
    [OnOpenAsset(0)]//executa o primeiro método por causa do 0
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        //checa se o grafo das salas tá vazio ou não, se não estiver preenche com as informações restantes
        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    //controle dos grafos através do GUILayout criado no OnEnable
    private void OnGUI() {

        //verifica se um objeto do tipo RoomNodeGraphSO foi selecionado
        if (currentRoomNodeGraph != null) {
            //desenha a linha que tá sendo puxada do nó
            DrawDraggedLine();

            //se sim, ele vai processar os eventos solicitados
            ProcessEvents(Event.current);

            //desenha a conexão entre dois nós
            DrawRoomConnections();

            //Gera os nós do grafo, representando cada sala
            DrawRoomNodes();
        }

        if (GUI.changed) {
            Repaint();
        }
    }

    private void DrawDraggedLine() {
        //vai desenhar a linha partindo do nó, até o mouse direito parar de ser clicado
        if (currentRoomNodeGraph.linePosition != Vector2.zero) {
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent) {

        //seleciona o nó que ou tá vazio ou não tá sendo arrastado
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false) {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        //verifica se a gnt não tá arrastando um nó, ou já tá arrastando outra linha
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null) {
            ProcessRoomNodeGraphEvents(currentEvent);
        } else {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    //função que verifica individualmente se o mouse está em cima do nó
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent) {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--) {
            //verifica se o mouse tá em cima da área retangular do nó(rect)
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition)) {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    //função que processa as ações
    private void ProcessRoomNodeGraphEvents(Event currentEvent) {
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

    //processa os eventos de clique no mouse dentro do grafo das salas
    private void ProcessMouseDownEvent(Event currentEvent) {
        
        //Processa o click com botão direito 
        if (currentEvent.button == 1) {
            ShowContextMenu(currentEvent.mousePosition);
        }
        //Processa o click com botão esquerdo
        else if (currentEvent.button == 0){

            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    //mostra o texto do menu para verificação dos grafos põs clique no mouse
    private void ShowContextMenu(Vector2 mousePosition) {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Cria novo nó pra sala"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }

    //cria uma sala na posição menu, tentar centralizar a entrada
    //igual em aeds2, um método vai chamar o outro de mesmo nome, mas com parametros adicionais
    private void CreateRoomNode(object mousePositionObject) {

        // Se o grafo está vazio cria a sala de entrada
        if (currentRoomNodeGraph.roomNodeList.Count == 0) {

            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.lista.Find(x => x.isEntrada));
        }
        
        CreateRoomNode(mousePositionObject, roomNodeTypeList.lista.Find(x => x.isNone));
    }

    //cria a lista de grafos adicionando cada sala criada
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType) {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        //cria uma asset pra cada nó de sala que tem o SO
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //adiciona o nó atual na lista de nó que a gente criou com todas as salas
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //coloca os valores certos em cada nó
        roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        //adiciona cada nó da sala no banco de dados do grafo
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        //reinicia a estrutura de dados
        currentRoomNodeGraph.OnValidate();
    }

    // Remove a seleção de todos  os nos
    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }

    }
    private void ProcessMouseUpEvent(Event currentEvent) {
        //se a gente arrasta a linha, mas não chega em nenhum outro nó, ela vai ser excluída
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null){

            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if(roomNode != null){
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddIdFilhoToRoomNode(roomNode.id)){
                    roomNode.AddIdPaiToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }
            ClearLineDrag();
        }
    }
    private void ProcessMouseDragEvent(Event currentEvent){
        //se clicar com o botão direito, vai chamar a função pra traçar a linha
        if(currentEvent.button == 1){
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessRightMouseDragEvent(Event currentEvent){
        if(currentRoomNodeGraph.roomNodeToDrawLineFrom != null){
            //chama a função pra desenhar a linha de acordo com o delta para gerenciar a posição
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    public void DragConnectingLine(Vector2 delta){
        currentRoomNodeGraph.linePosition += delta;
    }

    private void ClearLineDrag() {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    //essa função vai passar por todos os nós e fazer a comparação se o nó clicado está na estrutura, e alterando conforme os valores obtidos
    private void DrawRoomConnections() {
        //loop por todos os nós
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList) {
            if (roomNode.idListaNodeFilho.Count > 0) {
                //loop que vai percorrer por todos os nó filho
                foreach (string idNodeFilho in roomNode.idListaNodeFilho) {
                    //pega o nó do filho dentro da estrutura de dados
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(idNodeFilho)) {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[idNodeFilho]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    //essa função vai desenhar a ligação entre pai e filho
    private void DrawConnectionLine(RoomNodeSO idNodePai, RoomNodeSO idNodeFilho){
        //inicializa o nó pai e o nó filho
        Vector2 startPosition = idNodePai.rect.center;
        Vector2 endPosition = idNodeFilho.rect.center;

        //calcula o meio da conexão entre dois nós
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        Vector2 direction = endPosition - startPosition;

        //calcula as retas perpendiculares pra dar o aspecto de seta
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        //calcula onde tudo se encontra
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        //desenho da flecha
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        //desenha a linha dentro do editor
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    //função que vai desenhar nós do grafo selecionado
    private void DrawRoomNodes(){
        //loop até que todos os nós tenham sido desenhados
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList){

            if (roomNode.isSelected)
            {
                // cria o no que está selecionado
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                //chama o css do nó pra criar ele
                roomNode.Draw(roomNodeStyle);
            }
        }
        GUI.changed = true;
    }

    // Muda a tela do editor de grafo quando seleciona outro RoomNodeGraph
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }

    }

}
