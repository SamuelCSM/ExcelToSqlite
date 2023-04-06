using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;
using UnityEngine;

public class TestRef : MonoBehaviour
{
    public interface _iA
    {
        
    }

    public class A : _iA
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        CustomSerializerMgr.Instance.initCustomSerializer();
        SQLDataMgr.Instance.LoadDatabase();

        RefPerson p = SQLDataMgr.Instance.SQL.Table<RefPerson>().Where(x => x.Id == 2).FirstOrDefault();

        RefPerson person1 = RefPerson.GetDataByKey(1);
        RefPerson person2 = RefPerson.GetDataByKey(2);
        RefPerson person3 = RefPerson.GetDataByKey(3);
        RefEmail email3 = RefEmail.GetDataByKey(3);
        RefEmail email4 = RefEmail.GetDataByKey(4);
        RefEmail email5 = RefEmail.GetDataByKey(5);
        Debug.LogError(person1);
        Debug.LogError(person2);
        Debug.LogError(person3);
        Debug.LogError(email3);
        Debug.LogError(email4);
        Debug.LogError(email5);
    }

    // Update is called once per frame
    private bool show;
    private float time;
    void Update()
    {
        time += Time.deltaTime;
        if (!show)
        {
            if (time > 5)
            {
                //RefPerson p = SQLDataMgr.Instance.SQL.Table<RefPerson>().Where(x => x.Id == 2).FirstOrDefault();

                //RefPerson person1 = RefPerson.GetDataByKey(1);
                //RefPerson person2 = RefPerson.GetDataByKey(2);
                //RefPerson person3 = RefPerson.GetDataByKey(3);
                //Debug.LogError($"person1.name {person1.Name}");
                //Debug.LogError($"person2.Age {person2.Age}");
                //Debug.LogError($"person3.Surname {person3.Surname}");

                show = true;
            }
        }
    }
}
