using System.Xml.Serialization;

namespace BangumiSubReNamer.Models;

public class JsonDataClass
{
}

#region bgm api search

public class BgmApiJson_SearchListItem
{
    public float id { get; set; }
    public string name { get; set; }
    public string name_cn { get; set; }
    public float eps_count { get; set; }
    public string summary { get; set; }
    public string air_date { get; set; }
}

public class BgmApiJson_Search
{
    public float results { get; set; }
    public List<BgmApiJson_SearchListItem> list { get; set; }
}

#endregion

#region tmdb api search

public class TmdbApiJson_SearchResultsItem
{
    public int? id { get; set; }
    public string original_title { get; set; }
    public string original_name { get; set; }
    public string media_type { get; set; }
}

public class TmdbApiJson_Search
{
    public int? page { get; set; }
    public List<TmdbApiJson_SearchResultsItem> results { get; set; }
    public int? total_pages { get; set; }
    public int? total_results { get; set; }
}

#endregion

#region episodes

public class BgmApiJson_EpisodesInfoListItem
{
    public string name { get; set; }
    public string name_cn { get; set; }
    public float ep { get; set; }
    public float sort { get; set; }
    public float id { get; set; }
    public float subject_id { get; set; }
}

public class BgmApiJson_EpisodesInfo
{
    public List<BgmApiJson_EpisodesInfoListItem> data { get; set; }
    public float total { get; set; }
    public float limit { get; set; }
    public float offset { get; set; }
}

#endregion

#region nfo info

[XmlRoot("episodedetails")]
public class NfoInfo_EpisodesRoot
{
    public string bangumiid { get; set; }
    public string title { get; set; }
    public string originaltitle { get; set; }
    public string showtitle { get; set; }
    public string episode { get; set; }
    public string season { get; set; }
}

[XmlRoot("tvshow")]
public class NfoInfo_SubjectsRootTv
{
    public string bangumiid { get; set; }
    public string title { get; set; }
    public string originaltitle { get; set; }
    public string showtitle { get; set; }
    public string year{ get; set; }
}

[XmlRoot("movie")]
public class NfoInfo_SubjectsRootMovie
{
    public string bangumiid { get; set; }
    public string title { get; set; }
    public string originaltitle { get; set; }
    public string year{ get; set; }
}

#endregion