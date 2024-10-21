using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    // Vari�veis
    [SerializeField] private string nomeMapa;
    [SerializeField] private string nomeMenu;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOptions;
    [SerializeField] private GameObject painelAudio;
    [SerializeField] private GameObject painelControles;
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private Animator transition;
    [SerializeField] private float transitionTime = 1f;

    // M�todo para iniciar o jogo (carregar o mapa)
    public void Jogar()
    {
        // Inicia a transi��o e carrega o mapa ap�s o tempo de transi��o
        StartCoroutine(LoadLevelByName(nomeMapa));
    }

    // Carrega a pr�xima cena pelo �ndice
    private IEnumerator LoadLevel(int levelIndex)
    {
        // Aciona a anima��o de transi��o
        transition.SetTrigger("Start");

        // Espera pela dura��o da transi��o antes de carregar a cena
        yield return new WaitForSeconds(transitionTime);

        // Carrega a cena pelo �ndice
        SceneManager.LoadScene(levelIndex);
    }

    // Carrega uma cena pelo nome (usado para Jogar)
    private IEnumerator LoadLevelByName(string sceneName)
    {
        // Aciona a anima��o de transi��o
        transition.SetTrigger("Start");

        // Espera pela dura��o da transi��o antes de carregar a cena
        yield return new WaitForSeconds(transitionTime);

        // Carrega a cena pelo nome
        SceneManager.LoadScene(sceneName);
    }

    // M�todo para carregar o menu
    public void Menu()
    {
        StartCoroutine(LoadLevelByName(nomeMenu));
    }

    // usa a fun��o de toggle painel pq � melhor que o m�todo manual de escrever true e false, gasta menos linha
    public void AbrirOptions() => TogglePainel(painelMenuInicial, painelOptions);
    public void FecharOptions() => TogglePainel(painelOptions, painelMenuInicial);

    public void AbrirAudio() => TogglePainel(painelOptions, painelAudio);
    public void FecharAudio() => TogglePainel(painelAudio, painelOptions);

    public void AbrirControles() => TogglePainel(painelOptions, painelControles);
    public void FecharControles() => TogglePainel(painelControles, painelOptions);

    public void AbrirCreditos() => TogglePainel(painelMenuInicial, painelCreditos);
    public void FecharCreditos() => TogglePainel(painelCreditos, painelMenuInicial);

    // fun��o pra sair do jogo, mas s� funciona com ele completo
    public void Quit()
    {
        Debug.Log("Sair do Jogo"); //testar no editor
        Application.Quit(); 
    }

    // M�todo auxiliar para alternar entre dois pain�is
    private void TogglePainel(GameObject painelToHide, GameObject painelToShow)
    {
        painelToHide.SetActive(false);
        painelToShow.SetActive(true);
    }
}
