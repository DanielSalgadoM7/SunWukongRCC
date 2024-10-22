using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    // Variáveis
    [SerializeField] private string nomeMapa;
    [SerializeField] private string nomeMenu;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOptions;
    [SerializeField] private GameObject painelAudio;
    [SerializeField] private GameObject painelControles;
    [SerializeField] private GameObject painelCreditos;
    public LevelLoader levelLoader;

    // Método para iniciar o jogo (carregar o mapa)
    public void Jogar()
    {
        // Inicia a transição e carrega o mapa após o tempo de transição
        levelLoader.Transition(nomeMapa);
    }

    // usa a função de toggle painel pq é melhor que o método manual de escrever true e false, gasta menos linha
    public void AbrirOptions() => TogglePainel(painelMenuInicial, painelOptions);
    public void FecharOptions() => TogglePainel(painelOptions, painelMenuInicial);

    public void AbrirAudio() => TogglePainel(painelOptions, painelAudio);
    public void FecharAudio() => TogglePainel(painelAudio, painelOptions);

    public void AbrirControles() => TogglePainel(painelOptions, painelControles);
    public void FecharControles() => TogglePainel(painelControles, painelOptions);

    public void AbrirCreditos() => TogglePainel(painelMenuInicial, painelCreditos);
    public void FecharCreditos() => TogglePainel(painelCreditos, painelMenuInicial);

    // função pra sair do jogo, mas só funciona com ele completo
    public void Quit()
    {
        Debug.Log("Sair do Jogo"); //testar no editor
        Application.Quit(); 
    }

    // Método auxiliar para alternar entre dois painéis
    private void TogglePainel(GameObject painelToHide, GameObject painelToShow)
    {
        painelToHide.SetActive(false);
        painelToShow.SetActive(true);
    }
}
