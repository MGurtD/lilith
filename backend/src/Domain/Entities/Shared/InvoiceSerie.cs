namespace Domain.Entities.Shared;

public class InvoiceSerie : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public int NextNumber { get; set; } = 1;
    public int Length { get; set; }

    public string GenerateNext()
    {
        var nextNumberStr = NextNumber.ToString().PadLeft(Length, '0');
        NextNumber++;
        return $"{Prefix}{nextNumberStr}{Suffix}";
    }
}
