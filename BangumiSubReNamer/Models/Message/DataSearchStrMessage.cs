namespace BangumiSubReNamer.Models;

public class DataSearchStrMessage
{
    public DataSearchStrMessage(string searchStr)
    {
        SearchStr = searchStr;
    }

    public string SearchStr { get; set; }
}