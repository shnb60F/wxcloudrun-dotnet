namespace aspnetapp.Service;

/// <summary>
/// 获取强化功能
/// </summary>
public class EnhancementSelectionService
{
    readonly Random Random = new Random();
    Array values = Enum.GetValues(typeof(EnhancementType));

    public EnhancementType GetEnhancement()
    {
        return (EnhancementType)values.GetValue(Random.Next(values.Length));
    }

    public EnhancementType[] GetEnhancements(GuardersSession guardersSession)
    {
        EnhancementType[] enhancements = new EnhancementType[] { EnhancementType.ArcherAIDown1p, EnhancementType.ArcherDamageUp1p, EnhancementType.ArcherPiercingUp5p };//new EnhancementType[3];
        var enhancementCounts = new Dictionary<EnhancementType, int>();

        // 初始化每种 EnhancementType 的计数
        foreach (EnhancementType enhancement in values)
        {
            enhancementCounts[enhancement] = guardersSession.GetEnhancementCount(enhancement);
        }

        var tempSet = guardersSession.ReinforceableSet.ToList();
        int tempIndex = tempSet.Count;
        //i用于计数当前随机生成的数量
        do
        {
            int randomNumber = Random.Next(tempIndex);
            enhancements.Append(tempSet[randomNumber]);
            tempSet.Remove(tempSet[randomNumber]);
            tempIndex = tempSet.Count;
        } while (tempIndex > 0);

        return enhancements;
        // return new EnhancementType[] { EnhancementType.ArcherAIDown1p, EnhancementType.ArcherDamageUp1p, EnhancementType.ArcherPiercingUp5p };
    }
}



///以下为一段奇葩的，错误代码
// namespace aspnetapp.Service;

// /// <summary>
// /// 获取强化功能
// /// </summary>
// public class EnhancementSelectionService
// {
//     static readonly Random Random = new Random();
//     static Array values = Enum.GetValues(typeof(EnhancementType));

//     public static EnhancementType GetEnhancement()
//     {
//         return (EnhancementType)values.GetValue(Random.Next(values.Length));
//     }

//     public static EnhancementType[] GetEnhancements(GuardersSession guardersSession)
//     {
//         EnhancementType[] enhancements = new EnhancementType[3];
//         var enhancementCounts = new Dictionary<EnhancementType, int>();

//         // 初始化每种 EnhancementType 的计数
//         foreach (EnhancementType enhancement in values)
//         {
//             enhancementCounts[enhancement] = guardersSession.GetEnhancementCount(enhancement);
//         }

//         int[] randomNumbers = new int[3] { -1, -1, -1 };
//         ///i用于计数当前随机生成的数量
//         for (int i = 0; i < 3;)
//         {
//             int randomNumber = Random.Next(guardersSession.ReinforceableSet.Count);
//             if (randomNumbers.Contains(randomNumber))
//                 continue;
//             randomNumbers[i] = randomNumber;
//             enhancements[i] = guardersSession.ReinforceableSet[randomNumber];
//             i++;
//         }

//         return enhancements;
//     }
// }