using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;

[Table("person")]
public class RefPerson : SQLDataBase<int, RefPerson>
{

    [PrimaryKey, AutoIncrement]
    public int Id;

    public string Name;
    public string Surname;
    public List<int> Age;

    public List<CommonItem> item;

    public string getStr()
    {
        if (item == null)
            return "";

        string s = "";
        for (int i = 0; i < item.Count; i++)
        {
            s += item[i];
            s += ",";
        }
        return s;
    }

    public override string ToString()
    {
        return string.Format("[Person: Id={0}, Name={1},  Surname={2}, Age={3}, item={4}]", Id, Name, Surname, string.Join(",", Age.ToArray()), getStr());
    }
}