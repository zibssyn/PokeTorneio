using System.ComponentModel;

namespace PokeTorneio.Enums
{
    public enum ResultadoMelhorDe3
    {
        [Description("0x0")]
        ZeroAZero = 0, 

        [Description("1x1")]
        UmAUm,

        [Description("1x0")]
        UmAZero,

        [Description("2x0")]
        DoisAZero,

        [Description("2x1")]
        DoisAUm 
    }
}
