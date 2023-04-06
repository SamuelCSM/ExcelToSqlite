
using System.Collections;
using SQLite4Unity3d;

public enum ViewType
{
    Skin,
    Hero,
    Fashion,
}

[Table("email")]
public class RefEmail : SQLDataBase<int, RefEmail>
{

    [PrimaryKey]
    public int Id;

    public string Name;
    public bool IsOwen;
    public ViewType viewType;

    public override string ToString()
    {
        return string.Format("[Person: Id={0}, Name={1}, IsOwen={2}, viewType = {3}]", Id, Name, IsOwen, viewType);
    }

    public class CommonItem
    {
        public int type;
        public int subId;
    }
}
