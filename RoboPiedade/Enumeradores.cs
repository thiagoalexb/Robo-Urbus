using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboPiedade
{
    public enum EtapaConsulta
    {
        Navegando,
        SelecionarBairro,
        RecolherDados,
        TratarDados,
        InserirDados
    }

    public enum TipoDia
    {
        DiaUtil,
        Sabado,
        Domingo
    }

    public enum TipoSaida
    {
        Terminal,
        Bairro
    }
}
