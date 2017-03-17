# Unity3d-CSVDatabase
Super simple database feeding from CSV files for unity engine.

Allows you to have data in CSV files scanned and turned into CSVObject holding all that data as a dictionary of string + object (name + value).

Current features:
* Inheritance (depth of 1000 is the default limit)
* Keywords
* Editor data viewer(readonly)
* Multiple files, with arbitrary columns
* Value type support (values may be stored in converted (int,float..) but casted to Object form)

Future features:
* custom data injection (like FBX parameter array)
* hot reload
* queries (by path, by name, by attribute...)

# How it works
It scans a folder for CSV files, then parses them based on rules.
Columns define "fields" or variable names. First three columns are internal and mandatory, 
the rest are arbitrary and you can add as much as you need.

# Installation
Simply drop two files anywhere in your unity project.


# Usage
## Sheet rules
![](http://i.imgur.com/7VBXYMm.png)

Create a GameObject in the scene and add CSVDatabaseLoader to it.
By default the database will look in "Data/CSV" folder for .csv files, that is hardcoded for now, but you can change it easy.

* A1 - "table path", all the items inside this table (their names, or ID's) will inherit this "root path" so object "sword" will have a database path of "weapons/sword", each file has his own path.

* second row defines Fields (value names):
  * First three columns are reserved by the system and writing names there is not necessary
  * Other columns are free to be used 
  * add ":int" or other type (int, float, bool) to the end of the column name, to specify to which type to convert the value
  
* System columns:
  * column A - Name : object name, or id, used to identify object and should be unique.
  * column B - Parent : put other object's name - local if within the same file, or full path if in other file (armor/dragon_helmet)
  * column C - Custom : reserved for future use, you will have a long line of arbitrary key:value separated by ';' unique for each object, just in case you need to shove a bunch of unique additionional data for whatever uses 

* Inheritance - you can inherit any other object, that shares similar fields (not necessary tho, but will throw an error to notify you). You can inherit an object that inherits another object. Any field left empty in the csv will be inherited.
 
# Keywords

current keywords, put them in fields

* $name - the name of the object 
* $path - the path of the object

# Code

```csharp

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

    public string databaseID = "weapons/oldsword";
    public string Name;
    public int Cost;
    public float Damage;

	// Use this for initialization
	void Start () {
        Initialize();
	}
	
	void Initialize() {
        //way 1:
        //get object first
        CSVObject obj = CSVDatabase.GetObj(databaseID);
        Name = obj.GetValue("name") as string;

        //way 2:
        Cost = (int) CSVDatabase.GetValue(databaseID, "cost");
    }
}


```

# Advices
 * dont use capital letters, anywhere, this will avoid all kinds of errors.

# Viewer
CSVDatabaseLoader is also a viewer for database contents.
Simply launch and see the contents in its inspector:
![](http://i.imgur.com/QXt8QXW.png)
