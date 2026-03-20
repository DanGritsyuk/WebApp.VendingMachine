using System.ComponentModel;

namespace VendingMachine.Common.Entities.Enums
{
    public enum CoinDenomination
    {
        [Description(description: "1 ₽")]
        One = 1,

        [Description(description: "2 ₽")]
        Two = 2,

        [Description(description: "5 ₽")]
        Five = 5,

        [Description(description: "10 ₽")]
        Ten = 10
    }
}
