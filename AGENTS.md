# ufex

Universal File Explorer

## Overview

ufex is a cross-platform desktop application that allows users to view internal data structures and metadata of many different file formats. 

ufex includes an extendable database of file format signatures and can identify the file format of many different file formats. Additional XML files can be added to support more file formats.

ufex utilizes a plugin architecture to support loading .NET assemblies on the file that contain support for additional file formats (referred to as file type plugins).

## Technology Stack

* C#
* .NET Core 10
* Avalonia

## Code Style Guide

* Use tabs instead of spaces for indentation
* Follow C# best practices
* Use PascalCase for all class, method, and property names (including const and static readonly fields)
* Use camelCase for local variables and method parameters
* Prefer `[ x, y, z]` format for rows instead of `new object[] { x, y, z }` because it is shorter and more readable.
* Prefer explicit types (Int32, UInt16, etc.) when working with data read from files in file type plugins. 

## Project Structure

* src/
  * Ufex.API - Contains most of the core application code. Contains the base classes for file type plugins so it is a dependency of every file type plugin. Any code that a file type plugin needs to reference MUST be in here. We also must maintain backwards compatibility of the C# API in this project because it is referenced by plugins.
  * Ufex.FileType - Contains code for detecting file types and reading the file type database XML files. Referenced by Ufex.Desktop. 
  * Ufex.Desktop - Cross-platform Ufex desktop application. Currently built using Avalonia. Contains the main application code for the desktop application, including the UI code. References Ufex.API and Ufex.FileType.
  * Ufex.Controls.Avalonia - Contains Avalonia controls used in the desktop application. Referenced by Ufex.Desktop.
* ext/ - File type plugin projects. These are checked out independently from the main respository and the plugins in the directory will vary.
* config/ - Contains XML config files for file type signatures and color profiles. These are loaded at runtime by the application and can be edited by users to add support for more file types or define new color profiles.
