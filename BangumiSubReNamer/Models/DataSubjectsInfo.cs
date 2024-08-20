namespace BangumiSubReNamer.Models;

public class DataSubjectsInfo
{
    public float Id { get; }
    public string Name { get; }
    public string NameCn { get; }
    public float EpsCount { get; }
    public string Desc { get; }
    public string AirDate { get; }
    public string ShowText { get; }

    private const double Tolerance = 1e-6;

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
        return Math.Abs(Id - info.Id) < Tolerance && Name == info.Name && NameCn == info.NameCn &&
               Math.Abs(EpsCount - info.EpsCount) < Tolerance && AirDate.Equals(info.AirDate);
    }


    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(Name);
        hashCode.Add(NameCn);
        hashCode.Add(EpsCount);
        hashCode.Add(Desc);
        hashCode.Add(AirDate);
        hashCode.Add(ShowText);
        return hashCode.ToHashCode();
    }
}