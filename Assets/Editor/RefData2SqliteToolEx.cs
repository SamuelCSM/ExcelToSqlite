using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Data;
using SQLite4Unity3d;
using System.Text;
using System.Reflection;
using Excel;

public class RefData2SqliteToolEx : EditorWindow
{
    private const string Excel_Path = "Assets/ResourcesOut/Excel";
    private const string __DATA_BASE_NAME = "configdata.txt";

    /// <summary>
    /// 排除在外的文件名称
    /// </summary>
    private static List<string> except_class = new List<string>(new string[]{

                                                  });

    private static string[] __need_package_files = { };

    private static string inputFiles;

    [MenuItem("Tool/RefData2SqliteToolEx")]
    static void Init()
    {
        _recordTableType();
        RefData2SqliteToolEx window = (RefData2SqliteToolEx)EditorWindow.GetWindow(typeof(RefData2SqliteToolEx));
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Export Sqlite Data"))
        {
            if (EditorUtility.DisplayDialog("转换数据库表", "所有refdata转换过程时间比较长,建议使用下面单个打包的方法", "转换", "取消"))
            {
                ExportRefDataToSqlite();
            }
        }
        inputFiles = EditorGUILayout.TextField("路径", inputFiles);
        if (GUILayout.Button("Export Input Sqlite Data"))
        {
            ExportRefDataToSqlite(inputFiles);
        }
    }

    void ExportRefDataToSqlite(string _fileName)
    {
    }

    public void ExportRefDataToSqlite()
    {
        string[] files = Directory.GetFiles(Excel_Path, "*.xlsx", SearchOption.AllDirectories);
        string dbPath = string.Format(@"{0}/ResourcesOut/Excel/{1}", Application.dataPath, __DATA_BASE_NAME);

        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
        File.WriteAllText(dbPath, string.Empty);

        foreach (string file in files)
        {
            string reFilePath = file.Replace('\\', '/');

            string exitension = Path.GetExtension(reFilePath);
            //开启文件并进行读取
            string tmpPath = reFilePath.Replace(exitension, ".tmpX");
            //拷贝一个文件，避免开启excel时无法读取的问题
            File.Copy(reFilePath, tmpPath, true);
            //开启文件并进行读取
            //FileStream fs = File.OpenRead(tmpPath);

            using (var stream = File.Open(tmpPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateOpenXmlReader(stream))
                {
                    do
                    {
                        //存储列名称,用于分析数据并进行读取
                        List<string> columnNameList = new List<string>();
                        List<string> columnDetailInfoNList = new List<string>();
                        int lineIdx = 0;
                        while (reader.Read())
                        {
                            string tableName = reader.Name;
                            Type type = getTableType(tableName);
                            if (type == null)
                            {
                                lineIdx++;
                                continue;
                            }

                            //先预先处理一遍第一行，得到columnNameList
                            if (1 == lineIdx)
                            {
                                using (SQLiteConnection connection = new SQLiteConnection(dbPath))
                                {
                                    TableMapping map = connection.GetMapping(type);
                                    //取列名
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        //添加列名称, 第一行不能有空列，空列意味着隔断
                                        if (reader.IsDBNull(i))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            string columnName = reader.GetString(i);
                                            columnNameList.Add(columnName);

                                            TableMapping.Column colunm = map.FindColumnWithPropertyName(columnName);
                                            string sqlType = Orm.SqlType(colunm, false);
                                            var columnDefinition = $"{columnName} {sqlType}";
                                            if (colunm.IsPK)
                                            {
                                                columnDefinition += " PRIMARY KEY";
                                                if (colunm.IsAutoInc)
                                                {
                                                    columnDefinition += " AUTOINCREMENT";
                                                }
                                            }

                                            columnDetailInfoNList.Add(columnDefinition);
                                        }
                                    }

                                    connection.BeginTransaction();

                                    using (SQLiteCommand command = new SQLiteCommand(connection))
                                    {
                                        command.CommandText = "CREATE TABLE " + tableName + " (" + string.Join(", ", columnDetailInfoNList.ToArray()) + ")";
                                        command.ExecuteNonQuery();
                                        connection.Commit();
                                    }
                                    connection.Execute("VACUUM");
                                }
                            }
                            else if (lineIdx > 1)
                            {
                                List<string> values = new List<string>();
                                // 取数据
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    //添加列名称, 第一行不能有空列，空列意味着隔断
                                    if (reader.IsDBNull(i))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        string value = "'" + reader.GetString(i) + "'";
                                        values.Add(value);
                                    }
                                }

                                // 导入数据
                                using (SQLiteConnection connection = new SQLiteConnection(dbPath))
                                {
                                    connection.BeginTransaction();

                                    using (SQLiteCommand command = new SQLiteCommand(connection))
                                    {

                                        command.CommandText = "INSERT INTO " + tableName + " (" +
                                                              string.Join(", ", columnNameList.ToArray()) + ") VALUES (" +
                                                              string.Join(", ", values.ToArray()) + ")";
                                        command.ExecuteNonQuery();
                                        connection.Commit();
                                    }

                                    connection.Execute("VACUUM");
                                }
                            }

                            //增加行号
                            lineIdx++;
                        }
                    } while (reader.NextResult());
                }
            }
            //删除拷贝的文件
            File.Delete(tmpPath);
        }

        AssetDatabase.Refresh();
    }

    private static Dictionary<string, Type> _recordTableTypeDic = new Dictionary<string, Type>();
    private static void _recordTableType()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(RefBase<>));
        Type[] types = assembly.GetTypes();

        _recordTableTypeDic.Clear();
        foreach (Type type in types)
        {
            if (Attribute.IsDefined(type, typeof(TableAttribute)))
            {
                TableAttribute t = type.GetCustomAttribute<TableAttribute>();
                _recordTableTypeDic?.Add(t.Name, type);
            }
        }
    }

    private static Type getTableType(string _tableName)
    {
        Type t = null;
        _recordTableTypeDic.TryGetValue(_tableName, out t);
        return t;
    }

    //private class CreateTableCommand
    //{
    //    public string CommandText { get; private set; }

    //    public CreateTableCommand(PropertyInfo[] properties)
    //    {
    //        var tableName = typeof(T).GetCustomAttribute<TableAttribute>().Name;
    //        var columns = new List<string>();
    //        foreach (var property in properties)
    //        {
    //            var columnName = property.Name;
    //            var isPrimaryKey = property.GetCustomAttribute<PrimaryKeyAttribute>() != null;
    //            var columnType = SQLiteTypeMap.GetSQLiteType(property.PropertyType);
    //            var columnDefinition = $"{columnName} {columnType}";
    //            if (isPrimaryKey)
    //            {
    //                columnDefinition += " PRIMARY KEY";
    //                if (property.GetCustomAttribute<AutoIncrementAttribute>() != null)
    //                {
    //                    columnDefinition += " AUTOINCREMENT";
    //                }
    //            }
    //            columns.Add(columnDefinition);
    //        }
    //        CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(", ", columns)})";
    //    }
    //}
}
