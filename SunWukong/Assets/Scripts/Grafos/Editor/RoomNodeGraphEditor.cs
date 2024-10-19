//as vari�veis estar�o em sua maioria em ingl�s para facilitar o entendimento do c�digo, pq em portugu�s fica meio esquisito

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

    //valor de cada n�
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    //valor de cada linha que conecta os n�s
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
        // Adiciona ao inspector o envento de mudan�a de tela para o editor do RoomNodeGraph
        Selection.selectionChanged += InspectorSelectionChanged;

        //isso aqui � css dos bot�es que organizam o grafo das salas
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // "css" dos bot�es que foram selecionados
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
    [OnOpenAsset(0)]//executa o primeiro m�todo por causa do 0
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        //checa se o grafo das salas t� vazio ou n�o, se n�o estiver preenche com as informa��es restantes
        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    //controle dos grafos atrav�s do GUILayout criado no OnEnable
    private void OnGUI() {

        //verifica se um objeto do tipo RoomNodeGraphSO foi selecionado
        if (currentRoomNodeGraph != null) {
            //desenha a linha que t� sendo puxada do n�
            DrawDraggedLine();

            //se sim, ele vai processar os eventos solicitados
            ProcessEvents(Event.current);

            //desenha a conex�o entre dois n�s
            DrawRoomConnections();

            //Gera os n�s do grafo, representando cada sala
            DrawRoomNodes();
        }

        if (GUI.changed) {
            Repaint();
        }
    }

    private void DrawDraggedLine() {
        //vai desenhar a linha partindo do n�, at� o mouse direito parar de ser clicado
        if (currentRoomNodeGraph.linePosition != Vector2.zero) {
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent) {

        //seleciona o n� que ou t� vazio ou n�o t� sendo arrastado
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false) {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        //verifica se a gnt n�o t� arrastando um n�, ou j� t� arrastando outra linha
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null) {
            ProcessRoomNodeGraphEvents(currentEvent);
        } else {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    //fun��o que verifica individualmente se o mouse est� em cima do n�
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent) {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--) {
            //verifica se o mouse t� em cima da �rea retangular do n�(rect)
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition)) {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    //fun��o que processa as a��es
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
        
        //Processa o click com bot�o direito 
        if (currentEvent.button == 1) {
            ShowContextMenu(currentEvent.mousePosition);
        }
        //Processa o click com bot�o esquerdo
        else if (currentEvent.button == 0){

            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    //mostra o texto do menu para verifica��o dos grafos p�s clique no mouse
    private void ShowContextMenu(Vector2 mousePosition) {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Cria novo n� pra sala"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }

    //cria uma sala na posi��o menu, tentar centralizar a entrada
    //igual em aeds2, um m�todo vai chamar o outro de mesmo nome, mas com parametros adicionais
    private void CreateRoomNode(object mousePositionObject) {

        // Se o grafo est� vazio cria a sala de entrada
        if (currentRoomNodeGraph.roomNodeList.Count == 0) {

            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.lista.Find(x => x.isEntrada));
        }
        
        CreateRoomNode(mousePositionObject, roomNodeTypeList.lista.Find(x => x.isNone));
    }

    //cria a lista de grafos adicionando cada sala criada
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType) {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        //cria uma asset pra cada n� de sala que tem o SO
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //adiciona o n� atual na lista de n� que a gente criou com todas as salas
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //coloca os valores certos em cada n�
        roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        //adiciona cada n� da sala no banco de dados do grafo
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        //reinicia a estrutura de dados
        currentRoomNodeGraph.OnValidate();
    }

    // Remove a sele��o de todos  os nos
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
        //se a gente arrasta a linha, mas n�o chega em nenhum outro n�, ela vai ser exclu�da
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
        //se clicar com o bot�o direito, vai chamar a fun��o pra tra�ar a linha
        if(currentEvent.button == 1){
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessRightMouseDragEvent(Event currentEvent){
        if(currentRoomNodeGraph.roomNodeToDrawLineFrom != null){
            //chama a fun��o pra desenhar a linha de acordo com o delta para gerenciar a posi��o
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

    //essa fun��o vai passar por todos os n�s e fazer a compara��o se o n� clicado est� na estrutura, e alterando conforme os valores obtidos
    private void DrawRoomConnections() {
        //loop por todos os n�s
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList) {
            if (roomNode.idListaNodeFilho.Count > 0) {
                //loop que vai percorrer por todos os n� filho
                foreach (string idNodeFilho in roomNode.idListaNodeFilho) {
                    //pega o n� do filho dentro da estrutura de dados
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(idNodeFilho)) {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[idNodeFilho]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    //essa fun��o vai desenhar a liga��o entre pai e filho
    private void DrawConnectionLine(RoomNodeSO idNodePai, RoomNodeSO idNodeFilho){
        //inicializa o n� pai e o n� filho
        Vector2 startPosition = idNodePai.rect.center;
        Vector2 endPosition = idNodeFilho.rect.center;

        //calcula o meio da conex�o entre dois n�s
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

    //fun��o que vai desenhar n�s do grafo selecionado
    private void DrawRoomNodes(){
        //loop at� que todos os n�s tenham sido desenhados
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList){

            if (roomNode.isSelected)
            {
                // cria o no que est� selecionado
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                //chama o css do n� pra criar ele
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
