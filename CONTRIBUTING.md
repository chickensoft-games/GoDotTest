# Contributing

Thanks for helping! 

GoDotTest depends on [GoDotLog] and [GoDotCollections], but because both of those have test projects that depend on GoDotTest, we reference them locally to make it easier to update each package when the Godot SDK version changes in the `.csproj` files.

When working with GoDotTest, make sure you have clones of [GoDotLog] and [GoDotCollections] alongside GoDotTest, as shown in the following:

```
- your projects directory/
  - go_dot_log/
    - ...
  - go_dot_collections/
    - ...
  - go_dot_test/
    - this file, CONTRIBUTING.md
    - ...
```

Be sure to run `dotnet restore` in each project. Sometimes the projects can have trouble building if `dotnet restore` isn't run in the correct order, so you may have to play with the order to get it all working. Each project will also probably have to be opened in Godot and built at least once to generate the `.godot` build folders for in each project for C# to compile.

[GoDotLog]: https://github.com/chickensoft-games/go_dot_log
[GoDotCollections]: https://github.com/chickensoft-games/go_dot_collections
