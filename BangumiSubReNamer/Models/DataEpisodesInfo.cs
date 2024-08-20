namespace BangumiSubReNamer.Models;

public class DataEpisodesInfo
{
    public float Id { get; }
    public string Name { get; }
    public string NameCn { get; }
    public string SubjectName { get; }
    public string SubjectNameCn { get; }
    public float Ep { get; }
    public float Sort { get; }
    public float SubjectId { get; }
    public float Type { get; }

    public string Year { get; }
    public string ShowText { get; }

    private const double Tolerance = 1e-6;

    public DataEpisodesInfo(float id, string name, string nameCn, string subjectName, string subjectNameCn, float ep, float sort,
        float subjectId, float type, string year)
    {
        Id = id;
        Name = name;
        NameCn = nameCn;
        SubjectName = subjectName;
        SubjectNameCn = subjectNameCn;
        Ep = ep;
        Sort = sort;
        SubjectId = subjectId;
        Type = type;
        Year = year;
        ShowText = Type switch
        {
            0 => $"{SubjectNameCn}  本篇:{Sort}  {NameCn} ({Name})  id:{Id}",
            1 => $"{SubjectNameCn}  SP:{Sort}  {NameCn} ({Name})  id:{Id}",
            _ => $"{SubjectNameCn}  其他:{Ep}-{Sort}  {NameCn} ({Name})  id:{Id}"
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DataEpisodesInfo other) return false;

        return Math.Abs(Id - other.Id) < Tolerance && Name == other.Name && NameCn == other.NameCn && SubjectName == other.SubjectName
               && SubjectNameCn == other.SubjectNameCn && Math.Abs(Ep - other.Ep) < Tolerance && Math.Abs(Sort - other.Sort) < Tolerance
               && Year == other.Year && Math.Abs(SubjectId - other.SubjectId) < Tolerance;
    }


    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(Name);
        hashCode.Add(NameCn);
        hashCode.Add(SubjectName);
        hashCode.Add(SubjectNameCn);
        hashCode.Add(Ep);
        hashCode.Add(Sort);
        hashCode.Add(SubjectId);
        hashCode.Add(Type);
        hashCode.Add(Year);
        hashCode.Add(ShowText);
        return hashCode.ToHashCode();
    }
}