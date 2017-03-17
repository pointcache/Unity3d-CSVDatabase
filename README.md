# Unity3d-CSVDatabase
Super simple database feeding from CSV files for unity engine.

Allows you to have data in CSV file scanned and turned into CSVObject holding all that data as a dictionary of string + object (name + value).

Current features:
* Inheritance (depth of 1000 is the default limit)
* Keywords
* Editor data viewer(readonly)
* Multiple files, with arbitrary columns
* Value type support (values may be stored in converted (int,float..) but casted to Object form)

Future features:
* custom data injection (like FBX parameter array)
* hot reload

# How it works
It scans a folder for CSV files, then parses them based on rules.
Columns define "fields" or variable names. First three columns are internal and mandatory, 
the rest are arbitrary and you can add as much as you need.

# Usage
## Sheet rules
![](http://i.imgur.com/7VBXYMm.png)

* A1 - 
