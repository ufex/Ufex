# ufex

Universal File Explorer

## Overview

ufex is a cross-platform desktop application that allows users to view internal data structures and metadata of many different file formats. 

ufex includes an extendable database of file format signatures and can identify the file format of many different file formats. Additional XML files can be added to support more file formats.

ufex utilizes a plugin architecture to support loading .NET assemblies on the file that contain support for additional file formats.

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