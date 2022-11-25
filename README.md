# GoDotTest

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord](https://img.shields.io/badge/Chickensoft%20Discord-%237289DA.svg?style=flat&logo=discord&logoColor=white)][discord] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

C# test runner for Godot. Run tests from the command line, collect code coverage, and debug tests in VSCode.

For Godot 3.x, use versions `<= 1.0.0`. For Godot 4.x, use versions `> 1.0.0`.

## Installation

Find the latest version of [GoDotTest] on nuget.

Add the latest version of GoDotTest to your `*.csproj` file. Make sure to replace `*VERSION*` with the latest version.

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.GoDotTest" Version="*VERSION*" />
</ItemGroup>
```

You can use GoDotTest with C# 10 and Godot to run, debug, and collect code coverage for your project inside Godot.

For C# 10 to work, you need the dotnet 6 SDK installed. See what you have installed with `dotnet --info`. On mac, Godot 3 can have trouble finding .NET 6 if you have older SDK's installed, due to the dotnet [path search order][godot-dotnet-paths]. There are also a few [work-arounds][dotnet-path-workaround] available.

## Examples

Here's a simple test which does absolutely nothing. It can use the `TestScene` node available to it from its base class to manipulate the scene tree, if needed.

```csharp
using Godot;
using GoDotTest;

public class ExampleTest : TestClass {
  private readonly ILog _log = new GDLog(nameof(ExampleTest));

  public ExampleTest(Node testScene) : base(testScene) { }

  [SetupAll]
  public void SetupAll() => _log.Print("Setup everything");

  [Setup]
  public void Setup() => _log.Print("Setup");

  [Test]
  public void Test() => _log.Print("Test");

  [Cleanup]
  public void Cleanup() => _log.Print("Cleanup");

  [CleanupAll]
  public void CleanupAll() => _log.Print("Cleanup everything");
}
```

Below is the test execution output GoDoTest shows for its own tests: 

![test output](doc_assets/test_output.png)

## Setup

You can debug tests in Godot from Visual Studio Code. To do this, you will need to specify the `GODOT` environment variable for the following launch configurations and scripts to work correctly. The `GODOT` variable should point to the path of the Godot executable.

You will need to specify the `GODOT` environment variable in your `.zshrc` or `.bash_profile` file (or set it up manually on Windows).

```sh
# Dotnet
export DOTNET_CLI_TELEMETRY_OPTOUT=1 # Disable analytics
DOTNET_ROOT="/usr/local/share/dotnet"
# Mono
export PATH="/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono:$PATH"
export PATH="$HOME/.dotnet/tools:$PATH"
# For dotnet 6 SDK:
export PATH="/usr/local/share/dotnet:/usr/local/share/dotnet/sdk:$PATH"
# Godot
# Path go Godot executable, on mac it might look like this:
export GODOT="/Applications/Godot.app/Contents/MacOS/Godot"
```

## Debugging

The following `launch.json` file provides launch configurations to debug the game, debug all the tests, or debug the currently open test in Visual Studio Code. To debug the currently open test, make sure the class name of the test matches the file name, as is typical in C#.

### Godot 3.x Launch Configurations

> You can also just copy and paste `.vscode/launch.json` and `.vscode/tasks.json` from this repository into your own project that uses GoDotTest.

```jsonc
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Play in Editor",
      "preLaunchTask": "build",
      "type": "godot-mono",
      "mode": "playInEditor",
      "request": "launch"
    },
    {
      "name": "Play Scene in Editor",
      "preLaunchTask": "build",
      "type": "godot-mono",
      "mode": "executable",
      "request": "launch",
      "executable": "${env:GODOT}",
      "executableArguments": [
        "${fileDirname}/${fileBasenameNoExtension}.tscn"
      ]
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
        "--run-tests",
        "--quit-on-finish"
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
        "--run-tests=${fileBasenameNoExtension}",
        "--quit-on-finish"
      ],
      "preLaunchTask": "build"
    },
    {
      "name": "Launch",
      "type": "godot-mono",
      "request": "launch",
      "mode": "executable",
      "preLaunchTask": "build",
      "executable": "/Applications/Godot.app/Contents/MacOS/Godot",
      "executableArguments": [
        "--path",
        "${workspaceRoot}"
      ]
    },
    {
      "name": "Launch (Select Scene)",
      "type": "godot-mono",
      "request": "launch",
      "mode": "executable",
      "preLaunchTask": "build",
      "executable": "/Applications/Godot.app/Contents/MacOS/Godot",
      "executableArguments": [
        "--path",
        "${workspaceRoot}",
        "${command:SelectLaunchScene}"
      ]
    },
    {
      "name": "Attach",
      "type": "godot-mono",
      "request": "attach",
      "address": "localhost",
      "port": 23685
    }
  ]
}
```

**Note:** You will also need the accompanying `tasks.json` (below) to be able to build the game before running the debug configurations for testing.

```jsonc
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "--no-restore"
      ],
      "problemMatcher": "$msCompile",
      "presentation": {
        "echo": true,
        "reveal": "silent",
        "focus": false,
        "panel": "shared",
        "showReuseMessage": true,
        "clear": false
      }
    }
  ]
}

```

### Godot 4.x Launch Configurations

You can reuse the same `tasks.json` as shown above for 3.x, but you need a different `launch.json` file. Be sure to define the `GODOT4` environment variable on your system to point to the Godot 4 executable.

```javascript
{
  "version": "0.2.0",
  "configurations": [
    // For these launch configurations to work, you need to setup a GODOT
    // environment variable. On mac or linux, this can be done by adding
    // the following to your .zshrc, .bashrc, or .bash_profile file:
    // export GODOT="/Applications/Godot.app/Contents/MacOS/Godot"
    {
      "name": "Play",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${env:GODOT4}",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
    },
    {
      "name": "Debug Tests",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${env:GODOT4}",
      "args": [
        // These command line flags are used by GoDotTest to run tests.
        "--run-tests",
        "--quit-on-finish"
      ],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
    },
    {
      "name": "Debug Current Test",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${env:GODOT4}",
      "args": [
        // These command line flags are used by GoDotTest to run tests.
        "--run-tests=${fileBasenameNoExtension}",
        "--quit-on-finish"
      ],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
    },
  ]
}
```

## Testing a Scene

Create a `test` folder in your project and create a test scene in it. Add a C# script to the root of the test scene with the following contents:

```csharp
using System.Reflection;
using Godot;
using GoDotTest;

public class Tests : Node2D {
  public override async void _Ready()
    => await GoTest.RunTests(Assembly.GetExecutingAssembly(), this);
}
```

## Main Scene

In your main scene, you need to determine if tests should be run. GoDotTest relies on the presence of certain command line arguments to determine if tests should be run.

In your main scene, you should construct a test environment from the command
line arguments and determine if tests should be run. If they are, you can switch to the test scene. Otherwise, you can switch to the game scene. If you've written your own scene switching system, you can adapt this file to use that accordingly.

```csharp
using Godot;
using GoDotTest;

public class Main : Node2D {
  public override void _Ready() {
    var testEnv = TestEnvironment.From(OS.GetCmdlineArgs());
    if (testEnv.ShouldRunTests) {
      GetTree().ChangeScene("res://test/Tests.tscn");
    }
    else {
      GetTree().ChangeScene("res://scenes/Game.tscn");
    }
  }
}
```

## Logging

Make sure you add this to your `project.godot` file so you can see test logs when they're running.

```
[network]

; Required to see all the logs when tests are running!

limits/debugger_stdout/max_chars_per_second=200000
limits/debugger_stdout/max_messages_per_frame=500
limits/debugger_stdout/max_errors_per_second=500
limits/debugger_stdout/max_warnings_per_second=500
```

## Assertions and Mocking

GoDotTest is only a test provider and test execution system. Keeping the scope of GoDotTest small allows us to update it rapidly and ensure it's always working well with the latest Godot versions.

For mocking, we recommend [Moq] for Godot 3.x and [LightMock.Generator] for Godot 4.x (since Moq won't work in Godot 4 until the [collectible assemblies] support is merged). If you want LightMock's API to more closely resemble Moq's, you can also use Chickensoft's [LightMoq] adapter.

For integration tests, we recommend [GodotTestDriver]. GodotTestDriver allows you to create drivers that allow you to simulate input, wait for the next frame, interact with UI elements, create custom test drivers, etc.

## Coverage

If your code is configured correctly to switch to the test scene when `--run-tests` is passed in (see above), you can run all of your tests and generate code coverage while Godot is running.

![test coverage](doc_assets/test_coverage.png)

First, install [coverlet] and [reportgenerator].


```sh
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet tool install --global coverlet.console
# Do this too if you're on an M1 mac / ARMx64 system:
# Works around https://github.com/dotnet/efcore/issues/27787#issuecomment-1110061226
dotnet tool update --global coverlet.console
```

To run Godot with code coverage enabled, use a script like the following (or reference the local [`coverage.sh`](test/coverage.sh).

**Note:** On macOS, you may need to run `chmod +x ./coverage.sh` to add execution permissions before you are able to run the `coverage.sh` script.

### Code Coverage Bash Script for Godot 3.x

```sh
coverlet .mono/temp/bin/Debug/ --target $GODOT --targetargs \
  "--run-tests --quit-on-finish" --format "lcov" \
  --output ./coverage/coverage.info \
  --exclude-by-file "**/test/**/*.cs" # Don't collect coverage for the tests

reportgenerator \
  -reports:"./coverage/coverage.info" \
  -targetdir:"./coverage/report" \
  -reporttypes:Html

# Open the coverage report in your browser.
open coverage/report/index.html
```

### Code Coverage Bash Script for Godot 4.x

You'll need the latest version of coverlet (> 3.2.0) that hasn't been released yet. You can build coverlet from source by installing .NET 5 SDK and following their [contribution guidelines][coverlet-contribution].

You also need to pass the `--coverage` flag to Godot for GoDotTest to exit correctly. Godot 4's exit behavior doesn't play nicely with coverlet, so GoDotTest needs to know that it should force exit the process via the .NET API's instead of routing the exit request through Godot. This does cause a few error messages to appear as the process exits, but it does not cause any other problems.

```sh
# This requires a GODOT4 environment variable.

# Be sure to replace the PATH/TO/coverlet.console/... with the path to
# your newly built version of coverlet below.

dotnet PATH/TO/coverlet.console/bin/Debug/net5.0/coverlet.console.dll "./.godot/mono/temp/bin/Debug" --verbosity detailed \
  --target $GODOT4 \
  --targetargs "--run-tests --coverage --quit-on-finish" \
  --format "opencover" \
  --output "./coverage/coverage.xml" \
  --exclude "**/*.g.cs" \
  --exclude-by-file "**/scenes/**/*.cs" \
  --exclude-by-file "**/test/**/*.cs" \
  --exclude-by-file "**/*Microsoft.NET.Test.Sdk.Program.cs" \
  --exclude-assemblies-without-sources "missingall"

reportgenerator \
  -reports:"./coverage/coverage.xml" \
  -targetdir:"./coverage/report" \
  -reporttypes:Html
```

## How It Works

GoDotTest uses C# Reflection to find all classes in the current assembly that extend the `TestClass` it provides. It uses a `TestProvider` to find and load test suites (classes that extend `TestClass`) that can be run. It references a `TestEnvironment` that is created from the command line arguments given to the game/Godot and filters the test suites based on the presence of a test suite name, if given.


GoDotTest uses a `TestExecutor` to run methods in the order they are declared in a `TestClass`. Test methods are denoted with the `[Test]` attribute.

Test output is displayed by a `TestReporter` which responds to test events.

Auxiliary methods, such as `Setup` and `Cleanup` are run before and after each test, respectively. They can be specified with the `[Setup]` and `[Cleanup]` attributes on methods in a `TestClass`.

Additionally, any methods tagged with the `[SetupAll]` or `[CleanupAll]` attributes will be run once at the start of the test suite and once at the end, respectively.

GoDotTest will `await` any `async Task` test methods it encounters. Tests do not run in parallel, nor are there any plans to add that functionality. The focus of GoDotTest is to provide a simple, C#-first approach to testing in Godot that runs tests in a very simple and deterministic manner.

If you need to customize how tests are loaded and run, you can use the code in [`GoTest.cs`](src/GoTest.cs) as a starting point.

## Command Line Arguments

- `--run-tests`: The presence of this flag informs your game that tests should be run. If you've setup your main scene to redirect to the test scene when it finds this flag (as described above), you can use pass this flag in when running Godot from the command line (for debugging or CI/CD purposes) to run your test(s).
- `--quit-on-finish`: The presence of this flag indicates that the test runner should exit the application as soon as it is finished running tests.
- `--stop-on-error`: The presence of this flag indicates that the test runner should stop running tests when it encounters the first error in any test suite. Without this flag, it will attempt to run all of the test suites.
- `--sequential`: The presence of this flag indicates that subsequent test methods in a test suite should be skipped if an error occurs in a test suite method. Use this if your test methods rely on the previous test method completing successfully. This flag is ignored when using `--stop-on-error`. 
- `--coverage`: Required when running tests with the intent to collect coverage in Godot 4. Allows GoDotTest to force-exit so that coverlet picks up on the coverage correctly.

For more information about command line flags, see [`TestEnvironment.cs`](src/TestEnvironment.cs).

## Contributing

For information on contributing, see [CONTRIBUTING.md](CONTRIBUTING.md).

<!-- Links -->

[chickensoft-badge]: https://chickensoft.games/images/chickensoft/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord]: https://discord.gg/gSjaPgMmYW
[line-coverage]: https://raw.githubusercontent.com/chickensoft-games/go_dot_test/main/test/reports/line_coverage.svg
[branch-coverage]: https://raw.githubusercontent.com/chickensoft-games/go_dot_test/main/test/reports/branch_coverage.svg
[GoDotTest]: https://www.nuget.org/packages/Chickensoft.GoDotTest/
[dotnet-path-workaround]: https://github.com/godotengine/godot-proposals/issues/1941#issuecomment-1118648965
[godot-dotnet-paths]: https://github.com/godotengine/godot/blob/ade4e9320a6ca403b8053fe5828d3f9ce809338c/modules/mono/editor/GodotTools/GodotTools/Build/MsBuildFinder.cs#L122-L130
[coverlet]: https://github.com/coverlet-coverage/coverlet
[reportgenerator]: https://github.com/danielpalme/ReportGenerator
[lcov]: https://github.com/linux-test-project/lcov
[Shouldly]: https://github.com/shouldly/shouldly
[Moq]: https://github.com/moq/moq4
[coverlet-contribution]: https://github.com/coverlet-coverage/coverlet/blob/master/CONTRIBUTING.md
[GodotTestDriver]: https://github.com/derkork/godot-test-driver
[collectible assemblies]: https://github.com/godotengine/godot/issues/66060
[LightMoq]: https://github.com/chickensoft-games/LightMoq
[LightMock.Generator]: https://github.com/anton-yashin/LightMock.Generator
