# ufex gui

* Supports opening one file at a time
* Hex editor

## MainWindow

```
        |--------------------------------------------------------------------|
        |[icon] Universal File Explorer                            [-][M][X] |
        |--------------------------------------------------------------------|
Menu    |File Edit View Search Tools Help                                    |
        |--------------------------------------------------------------------|
Toolbar |[open] [close] | [Def] [Hex] [Dec] [Bin] [Asc]                      |
        |--------------------------------------------------------------------|
Tabs    |[Info] [Hex] [Preview] [Structure] [Validation]                     |
        |<Tab Content>                                                       |
        |--------------------------------------------------------------------|
        |<Status Bar>                                                        |
        |--------------------------------------------------------------------|
```

### Actions

#### Open File
Catch exceptions and display modal error messages throughout.

1. Display a loading message in the status bar
2. Reset the tabs and the content on the tabs
3. Update the title bar on the window to display "Universal File Explorer - <filename>"
4. Get the `FileInfo` using `Ufex.API.FileInfo.FromFilePath` function. Update the file info/attributes given the `Ufex.API.FileInfo` instance returned.
5. Determine the file type using the `Ufex.FileType.FileTypeManager` class (`GetFileType` function)
6. Display the file type on the Info tab - if the file type cannot be determined, display "Unknown File Type"
7. Open a read-only file stream for the file - we'll need to keep it open because the hex editor loads data as the user scrolls
8. FUTURE: initialize the hex viewer - we need to build this control
9. If the file type is known, check if there is an associated file type plugin. 
	* Use `FileTypeManager.GetFileTypeClassesByFileType` to get a `FILETYPE_CLASS` (use the first one returned for now)
	* If a `FILETYPE_CLASS` is found, call `FileTypeManager.GetNewClassInstance` to get the filetype instance that will be used to retrieve data to populate the rest of the UI.
	* If no `FILETYPE_CLASS` was found, stop here.
10. Run the ProcessFile function on the file type instance.
11. Toggle the visible tabs. ShowTechnical == Structure, ShowGraphic == Preview, ShowFileCheck == Validation
12. Set the number format on the file type instance based on the number format option currently select (default, hex, decimal, etc)
13. Call `FileType.GetQuickInfo()` to get the data (rows/columns) to populate the data grid on the "Info" tab with
14. If the structure tab is utilized (`ShowTechnical == true`) then populate the structure tab by retrieving the `TreeNodes`.

### Info Tab

The info tab contains 3 sections: file info, file attributes, and custom data table.
* The custom data table will usually have 3 columns but it may have more depending on the type of file that was loaded - the structure of the data table is determined by the file type handler.
* The custom data table should fill the vertical space in the tab (all the space available after the file info/file attributes sections)
* The Attributes and Timestamps are Group boxes
* The Attributes group box contains various with read-only checkboxes that will be checked if the attribute is true

```
File Type: <text box (full width)>
File Path: <text box (full width)>
File Name: <text box (50% width)> File Extension: <text box> File Size: <text box>

|-Attributes------------------------------------|    |-Timestamps------------------|
| [ ] Read-only [ ] Hidden     [ ] Archive      |    | Created:    <date> <time>   |
| [ ] Normal    [ ] Temporary  [ ] System       |    | Updated:    <date> <time>   |
| [ ] Encrypted [ ] Compressed [ ] Sparse File  |    | Accessed:   <date> <time>   |
|-----------------------------------------------|    |-----------------------------|

|--------------------------------------------------|
| <column 1> | <column 2>  | <column 3>            |
|--------------------------------------------------|
| <value 1>  | <value 2>   | <value 3>             |
|--------------------------------------------------|
| <value 1>  | <value 2>   | <value 3>             |
|--------------------------------------------------|

```

## Hex Tab

Hexadecimal viewer

## Preview Tab

The preview tab will display visual (raster or vector) representations of files.

For image files, the preview will display the image with zoom in/zoom out functionality.

## Structure Tab

The structure view will display a tree on the left that represents different parts of the file and on the right will be a technical view of the selected node in the tree.

Between the left/right sides there will be a slider that can be used to resize the left/right sides.

### Hierarchy View

The hierarchy view is used to display the internal structure of a file. If the file formathas nested structures, there will be one or more levels of child nodes.

The `Ufex.API.FileType` class instance has a `TreeNodes` property that will return a `Ufex.API.Tree.TreeNodeCollection` that represents the tree to display. The `TreenNodeCollection` includes `TreeNode` instances which represent the nodes in the tree.

The nodes can have images which are described by the `Ufex.API.Tree.TreeViewIcon` enum. These icon descriptors should be mapped to icons in the `FluentIcons.Avalonia` package. The icons should be displayed in the tree to the left of the node text.

Clicking a node in the tree should trigger a call to the `GetData` function on the `Ufex.API.FileType` class instance which will return a `Ufex.API.Table.TableData`.

### Data View

The right side will display a data table with a variable number of columns. In the future, we may support alternative right-side user interfaces.

The `Ufex.API.Table.TableData` instance returned by a node when it is clicked should be displayed in the data grid. The `TableData` instance will either be a `TextTableData` or `DynamicTableData`. The `DynamicTableData` is able to adjust it's values based on the current number format (hex, decimal, etc.).

When the user changes the selected number format, the table needs to be re-rendered (retrieve the data again).