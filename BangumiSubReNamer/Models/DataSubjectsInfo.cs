namespace BangumiSubReNamer.Models;

public class DataSubjectsInfo
{
    public float Id { get; set; }
    public string Name { get; set; }
    public string NameCn { get; set; }
    public float EpsCount { get; set; }
    public string Desc { get; set; }
    public string AirDate { get; set; }
    public string ShowText { get; set; }


    public DataSubjectsInfo(float id, string name, string nameCn, float epsCount, string desc, string airDate)
    {
        Id = id;
        Name = name;
        NameCn = nameCn;
        EpsCount = epsCount;
        Desc = desc;
        AirDate = airDate;
        ShowText = $"{NameCn} ({Name})  id:{Id}  话数:{EpsCount}  放送时间：{AirDate}";
    }

    public override bool Equals(object? other)
    {
        if (other is not DataSubjectsInfo info) return false;
        return Id == info.Id && Name == info.Name && NameCn == info.NameCn &&
               EpsCount == info.EpsCount && AirDate.Equals(info.AirDate);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, NameCn, EpsCount, AirDate);
    }
}