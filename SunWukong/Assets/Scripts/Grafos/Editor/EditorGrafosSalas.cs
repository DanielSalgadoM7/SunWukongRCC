using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorGrafosSalas: EditorWindow
{
    private GUIStyle nodeEstiloSala;

    //valor de cada nó
    private const float larguraNode = 160f;
    private const float alturaNode = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    [MenuItem("Editor de Grafos", menuItem = "Window/Grafos/EditorGrafosSalas")]
    private static void AbrirEditor()
    {
        GetWindow<EditorGrafosSalas>("Editor do Grafo das Salas");
    }

    //permite que a gente consiga desenhar os grafos na nova janela do projeto
    private void OnEnable()
    {
        nodeEstiloSala = new GUIStyle();
        nodeEstiloSala.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        nodeEstiloSala.normal.textColor = Color.white;
        nodeEstiloSala.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        nodeEstiloSala.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    }

    private void OnGUI()
    {
        //inicializa a area 1, vector é a área de inicialização
        GUILayout.BeginArea(new Rect(new Vector2(100f, 100f), new Vector2(larguraNode, alturaNode)), nodeEstiloSala);
        EditorGUILayout.LabelField("Node 1");
        GUILayout.EndArea();

        //inicializa a area 2
        GUILayout.BeginArea(new Rect(new Vector2(300f, 300f), new Vector2(larguraNode, alturaNode)), nodeEstiloSala);
        EditorGUILayout.LabelField("Node 2");
        GUILayout.EndArea();
    }
}
