using UnityEngine;
[System.Serializable]
public class Doorway 
{
    public Vector2Int position;
    public Orientation orientation;
    public GameObject doorPrefab;
    #region Header
    [Header("começa de cima esq")]
    #endregion
    public Vector2Int doorwayStartCopyPosition;
    #region Header
    [Header("Largura da porta")]
    #endregion
    public int doorwayCopiaLarguraTile;
    #region Header
    [Header("Altura da prota")]
    #endregion
    public int doorwayCopiaAlturaTile;
    [HideInInspector]
    public bool isConnected = false;
    [HideInInspector]
    public bool isUnavailable = false;
}
