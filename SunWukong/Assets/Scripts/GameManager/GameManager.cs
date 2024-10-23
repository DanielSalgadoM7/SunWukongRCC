using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : SingletonScript<GameManager>
{
    #region Tooltip
    [Tooltip("Preenche o mapa com os SO que a gente fez")]
    #endregion Tooltip
    [SerializeField] private List<NivelMapaSO> listaNivelMapa;

    #region Tooltip
    [Tooltip("Começa com o mapa no nivel 0")]
    #endregion Tooltip
    [SerializeField] private int indexNivelMapaAtual = 0;

    [HideInInspector] public GameState gameState;

    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    private void Update()
    {
        HandleGameState();

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameState = GameState.gameStarted;
        }
    }

    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:

                PlayNivelMapa(indexNivelMapaAtual);

                gameState = GameState.playingLevel;

                break;
        }
    }

    private void PlayNivelMapa(int listaIndexNivelMapa) { }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(listaNivelMapa), listaNivelMapa);
    }
#endif
    #endregion Validation
}
