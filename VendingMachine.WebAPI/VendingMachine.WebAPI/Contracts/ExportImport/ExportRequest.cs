namespace VendingMachine.WebAPI.Contracts.ExportImport
{
    public class ExportRequest
    {
        public List<int> DrinkIds { get; set; } = new();
        public bool UseLegacyFormat { get; set; }
    }
}
