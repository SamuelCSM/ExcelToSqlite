using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SQLite4Unity3d;

/// <summary>
/// 配置数据管理类
/// </summary>
public class SQLDataMgr : Singleton<SQLDataMgr> {
    private const string configDataFileName = "configdata.txt"; //配置表数据库文件名称

    //private SQLiteDB _sqliteDB;
    private SQLiteConnection sql;
    public SQLiteConnection SQL {
        get {
            return sql;
        }
    }


    string dbCachePath = "";
    public void LoadDatabase(string _databaseName = "")
    {
        string DatabaseName = "";
        if (_databaseName == "")
            DatabaseName = configDataFileName;
#if UNITY_EDITOR
        var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
            dataBaseIsDone = true;
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
		
#elif UNITY_STANDALONE_OSX
		var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath);

#endif

            Debug.Log("Database written");
        }

        var dbPath = filepath;
#endif
        dbCachePath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);
#if UNITY_STANDALONE
        dbCachePath = string.Format("{0}/{1}", Application.persistentDataPath, Guid.NewGuid().ToString() + ".txt");
#endif
        if (System.IO.File.Exists(dbCachePath))
        {
            System.IO.File.Delete(dbCachePath);
        }

        //TextAsset ta = Resources.Load<TextAsset>("configdata");
        //Debug.LogError(">>>>>>>>>>" + ta.text);
        //System.IO.File.WriteAllBytes(dbCachePath, ta.bytes);

        //sql = new SQLiteConnection(dbCachePath, SQLiteOpenFlags.ReadOnly);
        //dbPath = string.Format(@"Assets/StreamingAssets/{0}", "existing.db");
        dbPath = string.Format(@"Assets/ResourcesOut/Excel/{0}", configDataFileName);
        sql = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly);
    }

    /// <summary>
    /// 执行查询语句
    /// </summary>
    /// <param name="_query"></param>
    /// <returns></returns>
    public void Query<TKey, TValue>(ref TValue _t, TKey _key)
    {
        sql.QueryByPk<TValue>(ref _t, _key);
    }

    /// <summary>
    /// 执行非查询语句
    /// </summary>
    /// <param name="_cmdSql"></param>
    public void Execute(string _cmdSql, params object[] _args)
    {
        sql.Execute(_cmdSql, _args);
    }

    public void CloseSQLData()
    {
        if (sql != null)
            sql.Close();
        if (dbCachePath != "")
        {
            if (System.IO.File.Exists(dbCachePath))
            {
                System.IO.File.Delete(dbCachePath);
            }
        }
    }

    public TableMapping GetTableMapping(Type _type)
    {
        return sql.GetMapping(_type);
    }

    public int Count<T>() where T : new()
    {
        return sql.Table<T>().Count();
    }

    //整理数据库
    public int Vacuum()
    {
        return sql.Execute("VACUUM");
    }

    public List<T> GetAll<T>() where T : new()
    {
        if (sql == null)
        {
            return new List<T>();
        }
        return new List<T>(sql.Table<T>());
    }

    public IEnumerator<T> GetEnumerator<T>() where T : new()
    {
        return sql.Table<T>().GetEnumerator();
    }
}
