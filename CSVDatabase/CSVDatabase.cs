using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

[Serializable]
public class CSVObject {
    public CSVObject(string _name, string _path, string _parent) {
        name = _name;
        path = _path;
        parent = _parent;
    }
    public string name;
    public string path;
    public string parent;
    public Dictionary<string, object> name_value = new Dictionary<string, object>();

    public object GetValue(string field) {
        object val;
        name_value.TryGetValue(field, out val);
        if (val == null)
            Debug.LogError("CSVObject: such field was not found by the name:" + field);
        return val;
    }
}

public static class CSVDatabase {

    static string rootPath = Application.dataPath + "/Data/CSV/";
    public static Dictionary<string, CSVObject> database;


    public static CSVObject GetObj(string path) {
        CSVObject obj;
        database.TryGetValue(path, out obj);
        if (obj == null)
            Debug.LogError("CSVDATABASE: object not found by the path :" + path);
        return obj;
    }

    public static object GetValue(string path, string name) {
        CSVObject obj = GetObj(path);
        if (obj == null)
            return null;
        return obj.GetValue(name);
    }

    public static void Initialize() {
        database = new Dictionary<string, CSVObject>();
        List<CSVObject> toparent = new List<CSVObject>();
        //first pass - simply parse all the files into objects
        //get file list
        string[] files = Directory.GetFiles(rootPath, "*.csv");
        foreach (var f in files) {
            var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            try {
                using (StreamReader sr = new StreamReader(fs)) {
                    int linecount = -1;
                    int tableWidth = 0;
                    string tablePath = "";
                    string[] fieldnames = null;
                    string[] fieldtypes = null;
                    string[] fieldnamesplit;

                    while (!sr.EndOfStream) {
                        string[] line = SplitCSV(sr.ReadLine());
                        linecount++;
                        if (linecount == 0) {
                            //first line each csv file has its own "path root"
                            tablePath = line[0];
                            tableWidth = line.Length;
                            fieldnames = new string[tableWidth];
                            fieldtypes = new string[tableWidth];
                            continue;
                        } else
                        if (linecount == 1) {
                            //second line defines field names and field types
                            for (int i = 0; i < tableWidth; i++) {
                                //first part is name, second is type
                                fieldnamesplit = line[i].Split(':');
                                fieldnames[i] = fieldnamesplit[0];
                                if (fieldnamesplit.Length > 1)
                                    fieldtypes[i] = fieldnamesplit[1];
                            }
                            continue;
                        }
                        if (string.IsNullOrEmpty(line[0])) {
                            continue;
                        }
                        string objectDbName = line[0];
                        string parent = "";
                        if (!string.IsNullOrEmpty(line[1]))
                            parent = line[1].Contains("/") ? line[1] : tablePath + line[1];

                        CSVObject csvobj = new CSVObject(objectDbName, tablePath + objectDbName, parent);

                        //TODO add cutom column support

                        if (!string.IsNullOrEmpty(parent)) {
                            toparent.Add(csvobj);
                        }
                        //parse fields
                        for (int i = 3; i < line.Length; i++) {
                            csvobj.name_value.Add(fieldnames[i], ParseTo(line[i], fieldtypes[i], csvobj));
                        }
                        database.Add(csvobj.path, csvobj);
                    }
                }
            }
            catch (Exception e) {
                Debug.LogError("The file could not be read:");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
        }


        //INHERITANCE
        //1. reorder for proper hierarchical inheritance, we find out how deep is objects parent
        //chain and the add them to a list
        int count = toparent.Count;
        Dictionary<int, List<CSVObject>> orderedByDepth = new Dictionary<int, List<CSVObject>>();
        for (int i = 0; i < count; i++) {
            int depth = 0;
            CSVObject current = toparent[i];
            while (!string.IsNullOrEmpty(current.parent)) {
                depth++;
                database.TryGetValue(current.parent, out current);

                if (depth > 1000) {
                    Debug.LogError("CSVDATABASE: Most likely you have a circular dependency in " + current.path);
                    break;
                }
            }
            List<CSVObject> list = null;
            orderedByDepth.TryGetValue(depth, out list);
            if (list == null) {
                list = new List<CSVObject>();
                orderedByDepth.Add(depth, list);
            }
            list.Add(toparent[i]);
        }

        List<CSVObject> sortedToParent = new List<CSVObject>(count);
        int orderCount = orderedByDepth.Count;
        for (int i = 1; i < orderCount + 1; i++) {
            List<CSVObject> currentDepth;
            orderedByDepth.TryGetValue(i, out currentDepth);
            sortedToParent.AddRange(currentDepth);
        }
        //sorting complete

        for (int i = 0; i < count; i++) {


            //2. find parent
            CSVObject child = sortedToParent[i];
            CSVObject parent = null;
            database.TryGetValue(child.parent, out parent);
            if (parent == null) {
                Debug.LogError("CSVDATABASE: Could not find a parent for " + child.path);
                continue;
            }
            //3. inherit values
            var dictCopy = new Dictionary<string, object>(child.name_value);
            foreach (var pair in dictCopy) {
                if (pair.Value == null || (pair.Value is string) && (string.IsNullOrEmpty((string)pair.Value))) {
                    object parentVal;
                    parent.name_value.TryGetValue(pair.Key, out parentVal);
                    if (parentVal == null) {
                        Debug.LogError("CSVDATABASE: Could not locate the field you try to inherit in the parent " + child.path);
                        continue;
                    }
                    child.name_value[pair.Key] = parentVal;
                }
            }
        }
    }

    static bool ParentHasParent(CSVObject obj, CSVObject parent) {

        return !string.IsNullOrEmpty(parent.parent);
    }

    static object ParseTo(string value, string type, CSVObject obj) {
        switch (value) {
            case "$name":
                return obj.name;
            case "$path":
                return obj.path;
            default:
                break;
        }

        object result = null;

        switch (type) {
            case "int": {
                    int val;
                    if (Int32.TryParse(value, out val))
                        result = val;
                    else
                        result = 0;
                }
                break;
            case "float": {
                    float val;
                    if (Single.TryParse(value, out val))
                        result = Convert.ToSingle(value);
                    else
                        result = 0f;
                }
                break;
            case "bool": {
                    bool val;
                    if (Boolean.TryParse(value, out val))
                        result = Convert.ToBoolean(value);
                    else
                        result = false;
                }
                break;
            default:
                result = "";
                break;
        }
        return result == null ? value : result;
    }

    static List<string> swaplist = new List<string>();
    static Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.None);

    static string[] SplitCSV(string input) {
        swaplist.Clear();
        string curr = null;

        foreach (Match match in csvSplit.Matches(input)) {
            curr = match.Value;
            if (0 == curr.Length) {
                swaplist.Add("");
            }

            swaplist.Add(curr.TrimStart(','));
        }

        return swaplist.ToArray();
    }
}
