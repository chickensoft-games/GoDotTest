# GoDotTest

An opinionated test runner system to make running C# tests easier in Godot. Supports code coverage and debugging in-editor. 

GoDotTest depends on a related addon, `GoDotNet`, which is designed to make C# game development a little bit easier in Godot. 

## Debugging Tests in VSCode

Tests can be debugged in VSCode.

Steps:

- Create an environment variable on your system called `GODOT` which points to your Godot executable. On mac, you can just add something like this to your `.zshrc` file: `alias godot="/Applications/Godot.app/Contents/MacOS/Godot"`
- Configure your Visual Studio Code `launch.json` file correctly (see below).
- Create a test scene.
- Create an entry point scene for your project which creates a test environment based off the command line arguments given to the game. If the test environment indicates that tests should be run, switch to the test scene.
- In the test scene, execute tests by calling `GoTest.RunTests`.

### Configuring launch.json

```jsonc
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Play in Editor",
      "type": "godot-mono",
      "mode": "playInEditor",
      "request": "launch"
    },
    // We tell the game to run tests by using command line arguments.
    // This means we can't use the "play in editor" option — we have to launch
    // our own instance of Godot.
    //
    // Since passing scene files to Godot doesn't seem to work easily with the
    // C# Tools for Godot VSCode plugin, we use the path to Godot from the
    // environment. Make sure you set the GODOT variable to your Godot
    // executable.
    //
    // On mac, you can add the following to your zsh rc file:
    // alias godot="/Applications/Godot.app/Contents/MacOS/Godot"
    {
      "name": "Debug Tests",
      "type": "godot-mono",
      "mode": "executable",
      "request": "launch",
      "executable": "${env:GODOT}",
      "executableArguments": [
        "--run-tests"
      ],
      "preLaunchTask": "build"
    },
    // Debug the current test!
    //
    // The test runner will look for the class with the same name as the test
    // file that's currently open (disregarding its folder and file extension).
    // The search is case-insensitive.
    {
      "name": "Debug Current Test",
      "type": "godot-mono",
      "mode": "executable",
      "request": "launch",
      "executable": "${env:GODOT}",
      "executableArguments": [
        "--run-tests=${fileBasenameNoExtension}"
      ],
      "preLaunchTask": "build"
    },
  ]
}
```
