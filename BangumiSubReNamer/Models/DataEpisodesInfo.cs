namespace BangumiSubReNamer.Models;

public class DataEpisodesInfo
{
    public float Id { get; set; }
    public string Name { get; set; }
    public string NameCn { get; set; }
    public string SubjectName { get; set; }
    public string SubjectNameCn { get; set; }
    public float Ep { get; set; }
    public float Sort { get; set; }
    public float SubjectId { get; set; }
    public float Type { get; set; }

    public string Year { get; set; }
    public string ShowText { get; set; }

    public DataEpisodesInfo(float id, string name, string nameCn, string subjectName, string subjectNameCn, float ep, float sort,
        float subjectId, float type, string year)
    {
        Id = id;
        Name = name;
        NameCn = nameCn;
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

        return Id == other.Id && Name == other.Name && NameCn == other.NameCn && SubjectName == other.SubjectName
               && SubjectNameCn == other.SubjectNameCn && Ep == other.Ep && Sort == other.Sort && Year == other.Year &&
               SubjectId == other.SubjectId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, NameCn, SubjectNameCn, Ep, Sort, SubjectId, Type);
    }
}