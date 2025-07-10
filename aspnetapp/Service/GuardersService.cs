using System.Reflection.Metadata;
using aspnetapp.EF;
using aspnetapp.Entity;
using aspnetapp.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace aspnetapp.Service;

public class GuardersService
{
    readonly Random Random = new Random();
    Array values = Enum.GetValues(typeof(EnhancementType));

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="openId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<bool> Login(string openId, string name)
    {
        using GuardersContext guardersContext = new GuardersContext();
        UserDB? user = await guardersContext.Users.FindAsync(openId);
        if (user != null)
            return false;
        await guardersContext.Users.AddAsync(new UserDB
        {
            UserId = openId,
            UserName = name,
            UserAP = 30,
            UserCreatedAt = DateTime.Now,
            UserGold = 0,
            UserLevel = 0,
            UserXP = 0
        });
        // JsonConvert.SerializeObject(user);
        await guardersContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 用户注销
    /// </summary>
    /// <param name="openId"></param>
    /// <returns></returns>
    public async Task<bool> Logoff(string openId)
    {
        using GuardersContext guardersContext = new GuardersContext();
        UserDB? user = await guardersContext.Users.FindAsync(openId);
        if (user == null)
            return false;
        guardersContext.Users.Remove(user); // 直接删除找到的用户实例
        await guardersContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 获取用户全部Guarder
    /// </summary>
    /// <param name="openId"></param>
    /// <returns></returns>
    public async Task<ICollection<GuarderDB>> GuarderGetAll(string openId)
    {
        using GuardersContext guardersContext = new GuardersContext();

        UserDB? userDB = await guardersContext.Users.AsNoTracking().Include(u => u.Guarders).FirstOrDefaultAsync(t => t.UserId == openId);
        if (userDB == null)
            throw new NotFiniteNumberException($"userId({openId}) is unfound in Users");
        else if (userDB.Guarders == null)
            throw new NotFiniteNumberException($"userId({openId})'s Guarders is unfound in Users");

        return userDB.Guarders;
    }

    /// <summary>
    /// 获取用户全部Guarder
    /// </summary>
    /// <param name="openId"></param>
    /// <returns></returns>
    public async Task<UserDB> UserGetAll(string openId)
    {
        using GuardersContext guardersContext = new GuardersContext();

        UserDB? userDB = await guardersContext.Users.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == openId);
        if (userDB == null)
            throw new NotFiniteNumberException($"userId({openId}) is unfound in Users");

        return userDB;
    }

    /// <summary>
    /// Guarder新建
    /// </summary>
    /// <param name="openId"></param>
    /// <param name="guarderType"></param>
    /// <returns></returns>
    public async Task<bool> GuarderCreate(string openId, GuarderType guarderType)
    {
        using GuardersContext guardersContext = new GuardersContext();
        UserDB? user = await guardersContext.Users.Include(u => u.Guarders).FirstOrDefaultAsync(u => u.UserId == openId);
        if (user == null)
            return false;
        GuarderDB? guarder = user.Guarders.FirstOrDefault(g => g.GuarderType == guarderType);
        if (guarder != null)
            return false;

        if (guarderType != GuarderType.FireHoushi || user.UserLevel < 1)
            return false;

        await guardersContext.AddAsync(new GuarderDB { GuarderKakera = 0, GuarderLevel = 0, GuarderType = guarderType, UserId = openId });
        await guardersContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<GuarderDB>> GuarderSelect(string openId)
    {
        using GuardersContext guardersContext = new GuardersContext();
        return await guardersContext.Guarders
                        .AsNoTracking()
                        .Where(g => g.UserId == openId)
                        .ToListAsync();
    }

    /// <summary>
    /// Guarder升级
    /// </summary>
    /// <param name="openId"></param>
    /// <param name="guarderType"></param>
    /// <returns></returns>
    public async Task<bool> GuarderLevelUp(string openId, GuarderType guarderType)
    {
        using GuardersContext guardersContext = new GuardersContext();
        GuarderDB? guarder = await guardersContext.Guarders.Include(g => g.User).FirstOrDefaultAsync(g => g.UserId == openId && g.GuarderType == guarderType);
        if (guarder == null || (guarder.GuarderLevel + 1) * 10 > guarder.GuarderKakera || (guarder.GuarderLevel + 1) * 100 > guarder.User.UserGold)
            return false;
        guarder.GuarderKakera -= (guarder.GuarderLevel + 1) * 10;
        guarder.User.UserGold -= (guarder.GuarderLevel + 1) * 100;
        guarder.GuarderLevel++;
        await guardersContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 返回可强化集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<EnhancementType>?> QuestionCreate(string userId)
    {
        using GuardersContext guardersContext = new GuardersContext();
        UserDB? userDB = await guardersContext.Users.Include(u => u.Guarders).FirstOrDefaultAsync(u => u.UserId == userId);
        if (userDB == null || userDB.Guarders == null)
            return null;

        Dictionary<string, IEntity> entitys = new Dictionary<string, IEntity>();
        List<EnhancementType> ReinforceableSet = new List<EnhancementType>();

        foreach (var guarderDB in userDB.Guarders)
        {
            switch (guarderDB.GuarderType)
            {
                case GuarderType.Archer:
                    ReinforceableSet.Add(EnhancementType.ArcherAIDown1p);
                    ReinforceableSet.Add(EnhancementType.ArcherDamageUp1p);
                    ReinforceableSet.Add(EnhancementType.ArcherPiercingUp5p);
                    ReinforceableSet.Add(EnhancementType.ArcherShoutCountUp10p);
                    break;
                case GuarderType.FireHoushi:
                    ReinforceableSet.Add(EnhancementType.FireHouShiUnLock);
                    break;
            }
        }
        return ReinforceableSet;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reinforceableSet"></param>
    /// <param name="enhancements"></param>
    /// <returns></returns>
    public List<EnhancementType> GetEnhancements(List<EnhancementType> reinforceableSet, List<EnhancementType> enhancements)
    {
        var tempSet = reinforceableSet.ToList();
        var resultSet = new List<EnhancementType>();
        int tempIndex = tempSet.Count;
        int i = 0;
        int randomNumber = 0;

        while (i < 3)
        {
            randomNumber = Random.Next(tempIndex);
            switch (tempSet[randomNumber])
            {
                case EnhancementType.ArcherAIDown1p:
                    if (tempSet.Contains(EnhancementType.ArcherAIDown1p) && enhancements.Count(e => e == EnhancementType.ArcherAIDown1p) < 5)
                    {
                        Progress(resultSet, tempSet, randomNumber, ref i, ref tempIndex);
                        break;
                    }
                    break;
                case EnhancementType.ArcherDamageUp1p:
                    if (tempSet.Contains(EnhancementType.ArcherDamageUp1p))
                    {
                        Progress(resultSet, tempSet, randomNumber, ref i, ref tempIndex);
                        break;
                    }
                    break;
                case EnhancementType.ArcherPiercingUp5p:
                    if (tempSet.Contains(EnhancementType.ArcherPiercingUp5p))
                    {
                        Progress(resultSet, tempSet, randomNumber, ref i, ref tempIndex);
                        break;
                    }
                    break;
                case EnhancementType.ArcherShoutCountUp10p:
                    if (tempSet.Contains(EnhancementType.ArcherShoutCountUp10p))
                    {
                        Progress(resultSet, tempSet, randomNumber, ref i, ref tempIndex);
                        break;
                    }
                    break;
                case EnhancementType.FireHouShiDamageUp1p:
                    if (tempSet.Contains(EnhancementType.FireHouShiDamageUp1p))
                    {
                        Progress(resultSet, tempSet, randomNumber, ref i, ref tempIndex);
                        break;
                    }
                    break;
                case EnhancementType.FireHouShiUnLock:
                    if (tempSet.Contains(EnhancementType.FireHouShiUnLock))
                    {
                        Progress(resultSet, tempSet, randomNumber, ref i, ref tempIndex);
                        break;
                    }
                    break;
            }
        }

        return resultSet;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reinforceableSet">传出参数</param>
    /// <param name="enhancements">传出参数</param>
    /// <param name="enhancement"></param>
    public void Enhancement(List<EnhancementType> reinforceableSet, List<EnhancementType> enhancements, EnhancementType enhancement)
    {
        switch (enhancement)
        {
            case EnhancementType.ArcherAIDown1p:
                if (reinforceableSet.Contains(EnhancementType.ArcherAIDown1p))
                {
                    enhancements.Add(EnhancementType.ArcherAIDown1p);
                    break;
                }
                break;
            case EnhancementType.ArcherDamageUp1p:
                if (reinforceableSet.Contains(EnhancementType.ArcherAIDown1p))
                {
                    enhancements.Add(EnhancementType.ArcherDamageUp1p);
                    break;
                }
                break;
            case EnhancementType.ArcherPiercingUp5p:
                if (reinforceableSet.Contains(EnhancementType.ArcherPiercingUp5p))
                {
                    enhancements.Add(EnhancementType.ArcherPiercingUp5p);
                    break;
                }
                break;
            case EnhancementType.ArcherShoutCountUp10p:
                if (reinforceableSet.Contains(EnhancementType.ArcherShoutCountUp10p))
                {
                    enhancements.Add(EnhancementType.ArcherShoutCountUp10p);
                    break;
                }
                break;
            case EnhancementType.FireHouShiDamageUp1p:
                if (reinforceableSet.Contains(EnhancementType.FireHouShiDamageUp1p))
                {
                    enhancements.Add(EnhancementType.FireHouShiDamageUp1p);
                    break;
                }
                break;
            case EnhancementType.FireHouShiUnLock:
                if (reinforceableSet.Contains(EnhancementType.FireHouShiUnLock))
                {
                    reinforceableSet.Remove(EnhancementType.FireHouShiUnLock);
                    reinforceableSet.Add(EnhancementType.FireHouShiDamageUp1p);
                    enhancements.Add(EnhancementType.FireHouShiUnLock);
                    break;
                }
                break;
        }
    }

    /// <summary>
    /// 关卡完成；收获经验，宝箱
    /// </summary>
    /// <param name="question"></param>
    /// <param name="treasureBox"></param>
    /// <param name="userXP"></param>
    /// <returns></returns>
    public async Task<bool> QuestionClear(QuestionDB question, TreasureBoxDB treasureBoxDB, int userXP)
    {
        using GuardersContext guardersContext = new GuardersContext();

        UserDB? userDB = await guardersContext.Users.Include(u => u.Questions).FirstOrDefaultAsync(q => q.UserId == question.UserId);
        QuestionDB? questionDB = userDB?.Questions.FirstOrDefault(q => q.QuestionID == question.QuestionID);
        if (questionDB == null)
        {
            questionDB = question;
            await guardersContext.Questions.AddAsync(questionDB);
        }
        else
        {
            questionDB = question;
        }

        if (questionDB == null || userDB == null)
            return false;
        userDB.UserXP += userXP;
        await guardersContext.TreasureBoxs.AddAsync(treasureBoxDB);
        await guardersContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 宝箱开启
    /// </summary>
    /// <param name="question"></param>
    /// <param name="treasureBox"></param>
    /// <param name="userXP"></param>
    /// <returns></returns>
    public async Task<bool> TreasureBoxOpen(string openId, string treasureBoxId, long usergold, (GuarderType, int)[] guarderkakeras)
    {
        using GuardersContext guardersContext = new GuardersContext();
        UserDB? user = await guardersContext.Users
                                         .Include(u => u.Guarders)
                                         .Include(u => u.TreasureBoxs)
                                         .FirstOrDefaultAsync(u => u.UserId == openId);
        if (user == null || user.TreasureBoxs == null)
            return false;
        TreasureBoxDB? treasureBox = user.TreasureBoxs.FirstOrDefault(t => t.TreasureBoxId == treasureBoxId);
        if (treasureBox == null)
            return false;
        user.UserGold += usergold;
        foreach (var item in guarderkakeras)
        {
            GuarderDB? guarder = user.Guarders.FirstOrDefault(g => g.GuarderType == item.Item1);
            if (guarder == null)
                return false;
            guarder.GuarderKakera += item.Item2;
        }
        user.TreasureBoxs.Remove(treasureBox);
        await guardersContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// User等级提升
    /// </summary>
    /// <param name="openId"></param>
    /// <returns></returns>
    public async Task<bool> UserLevelUp(string openId)
    {
        using GuardersContext guardersContext = new GuardersContext();
        UserDB? user = await guardersContext.Users.FindAsync(openId);
        if (user == null || (user.UserLevel + 1) * 100 > user.UserXP)
            return false;
        user.UserXP -= (user.UserLevel + 1) * 100;
        user.UserLevel++;
        await guardersContext.SaveChangesAsync();
        return true;
    }


    /// <summary>
    /// Guarder碎片增加
    /// </summary>
    /// <param name="openId"></param>
    /// <param name="guarderType"></param>
    /// <param name="guarderKakera"></param>
    /// <returns></returns>
    public async Task<bool> GuarderKakeraUp(string openId, GuarderType guarderType, int guarderKakera)
    {
        using GuardersContext guardersContext = new GuardersContext();
        GuarderDB? guarder = await guardersContext.Guarders.Include(g => g.User).FirstOrDefaultAsync(g => g.UserId == openId && g.GuarderType == guarderType);
        if (guarder == null)
            return false;
        guarder.GuarderKakera += guarderKakera;
        await guardersContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// User经验上升
    /// </summary>
    /// <param name="openId"></param>
    /// <param name="userXP"></param>
    /// <returns></returns>
    public async Task<bool> UserXPUp(string openId, int userXP)
    {
        using GuardersContext guardersContext = new GuardersContext();
        UserDB? user = await guardersContext.Users.FindAsync(openId);
        if (user == null)
            return false;
        user.UserXP += userXP;
        await guardersContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// User金币增加
    /// </summary>
    /// <param name="openId"></param>
    /// <param name="userCold"></param>
    /// <returns></returns>
    public async Task<bool> UserGoldUp(string openId, long userCold)
    {
        using GuardersContext guardersContext = new GuardersContext();
        UserDB? user = await guardersContext.Users.FindAsync(openId);
        if (user == null)
            return false;
        user.UserGold += userCold;
        await guardersContext.SaveChangesAsync();
        return true;
    }

    void Progress(List<EnhancementType> resultSet, List<EnhancementType> reinforceableSet, int randomNumber, ref int index, ref int reinforceableSetIndex)
    {
        resultSet.Add(reinforceableSet[randomNumber]);
        reinforceableSet.RemoveAt(randomNumber);
        index++;
        reinforceableSetIndex--;
    }
}