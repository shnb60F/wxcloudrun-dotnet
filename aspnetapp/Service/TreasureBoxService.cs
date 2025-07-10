using aspnetapp.EF;
using Microsoft.EntityFrameworkCore;

namespace aspnetapp.Service;

/// <summary>
/// 获取宝箱功能
/// </summary>
public class TreasureBoxService
{
    readonly Random Random = new Random();
    Array values = Enum.GetValues(typeof(GuarderType));
    /// <summary>
    /// 普通宝箱概率基数
    /// </summary>
    readonly double Common = 8.0 / 15;
    /// <summary>
    /// 稀有宝箱概率基数
    /// </summary>
    readonly double Rare = 4.0 / 15;
    /// <summary>
    /// 史诗宝箱概率基数
    /// </summary>
    readonly double Epic = 2.0 / 15;
    /// <summary>
    /// 传说宝箱概率基数
    /// </summary>
    readonly double Legendary = 1.0 / 15;

    /// <summary>
    /// 获取全部宝箱
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ICollection<TreasureBoxDB>> GetAll(string userId)
    {
        using GuardersContext guardersContext = new GuardersContext();

        UserDB? userDB = await guardersContext.Users.AsNoTracking().Include(u => u.TreasureBoxs).FirstOrDefaultAsync(t => t.UserId == userId);
        if (userDB == null)
            throw new NotFiniteNumberException($"userId({userId}) is unfound in Users");

        return userDB.TreasureBoxs;
    }

    /// <summary>
    /// 生成随机品质宝箱
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public TreasureBoxDB GetBox(string userId)
    {
        string treasureBoxId = $"{Guid.NewGuid()}";
        TreasureBoxType treasureBoxType = GetTreasureBoxType();

        return new TreasureBoxDB
        {
            TreasureBoxCreateAt = DateTime.Now,
            TreasureBoxId = treasureBoxId,
            TreasureBoxType = treasureBoxType,
            UserId = userId
        };
    }

    /// <summary>
    /// 向数据库添加宝箱
    /// </summary>
    /// <param name="treasureBoxDB"></param>
    /// <returns></returns>
    /// <exception cref="NotFiniteNumberException"></exception>
    public async Task AddBox(TreasureBoxDB treasureBoxDB)
    {
        using GuardersContext guardersContext = new GuardersContext();

        string userId = treasureBoxDB.UserId;
        UserDB? userDB = await guardersContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        if (userDB == null)
            throw new NotFiniteNumberException($"userId({userId}) is unfound in Users");

        await guardersContext.TreasureBoxs.AddAsync(treasureBoxDB);
        await guardersContext.SaveChangesAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="treasureBoxId"></param>
    /// <returns>UserGold, GuarderKakera</returns>
    /// <exception cref="NotFiniteNumberException"></exception>
    public async Task<(long, GuarderType, long)> OpenBox(string treasureBoxId)
    {
        using GuardersContext guardersContext = new GuardersContext();

        TreasureBoxDB? treasureBoxDB = await guardersContext.TreasureBoxs.Include(t => t.User.Guarders).FirstOrDefaultAsync(t => t.TreasureBoxId == treasureBoxId);
        if (treasureBoxDB == null)
            throw new NotFiniteNumberException($"treasureId({treasureBoxId}) is unfound in TreasureBoxs");

        //奖励乘数
        int minUserGold = 0;
        int maxUserGold = 0;
        int minGuarderKakera = 0;
        int maxGuarderKakera = 0;
        switch (treasureBoxDB.TreasureBoxType)
        {
            case TreasureBoxType.Common:
                minUserGold = 10;
                maxUserGold = 30;
                minGuarderKakera = 1;
                maxGuarderKakera = 3;
                break;
            case TreasureBoxType.Rare:
                minUserGold = 20;
                maxUserGold = 60;
                minGuarderKakera = 2;
                maxGuarderKakera = 6;
                break;
            case TreasureBoxType.Epic:
                minUserGold = 40;
                maxUserGold = 120;
                minGuarderKakera = 4;
                maxGuarderKakera = 12;
                break;
            case TreasureBoxType.Legendary:
                minUserGold = 80;
                maxUserGold = 240;
                minGuarderKakera = 8;
                maxGuarderKakera = 24;
                break;
        }
        UserDB? userDB = treasureBoxDB.User;
        if (userDB == null || userDB.Guarders == null)
            throw new NotFiniteNumberException($"treasureBoxDB.UserId({treasureBoxDB.UserId}) is unfound in Users");
        GuarderType guarderType = (GuarderType)values.GetValue(Random.Next(userDB.Guarders.Count));
        GuarderDB? guarderDB = userDB.Guarders.FirstOrDefault(g => g.UserId == userDB.UserId && g.GuarderType == guarderType);
        if (guarderDB == null)
            throw new NotFiniteNumberException($"userDB.UserId({userDB.UserId}) and guarderType({guarderType}) is unfound in Guarders");

        int userGold = Random.Next(minUserGold, maxUserGold);
        int guarderKakera = Random.Next(minGuarderKakera, maxGuarderKakera);
        userDB.UserGold += userGold;
        guarderDB.GuarderKakera += guarderKakera;

        guardersContext.TreasureBoxs.Remove(treasureBoxDB);
        await guardersContext.SaveChangesAsync();
        return (userGold, guarderType, guarderKakera);
    }

    /// <summary>
    /// 生成随机TreasureBoxType
    /// </summary>
    /// <returns></returns>
    TreasureBoxType GetTreasureBoxType()
    {
        double randomValue = Random.NextDouble();
        if (randomValue < Common)
            return TreasureBoxType.Common;
        else if (randomValue < Common + Rare)
            return TreasureBoxType.Rare;
        else if (randomValue < Common + Rare + Epic)
            return TreasureBoxType.Epic;
        else
            return TreasureBoxType.Legendary;
    }
}