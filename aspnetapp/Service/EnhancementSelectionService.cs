using System;

public class EnhancementSelectionService
{
    static readonly Random Random = new Random();
    static Array values = Enum.GetValues(typeof(EnhancementType));

    public static EnhancementType GetEnhancement()
    {
        return (EnhancementType)values.GetValue(Random.Next(values.Length));
    }
}