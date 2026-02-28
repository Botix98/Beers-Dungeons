using UnityEngine;

public class HechizoATQ : Atributos
{

    [Header("Ataque")]
    public elementos elemento;
    protected int dano = 0;
    public int golpear;
    public float multiplicador;

    public void EstablecerDano(int valorDelNumero)
    {
        dano = valorDelNumero;
    }

    public int ObtenerDano()
    {
        return dano;
    }
}
