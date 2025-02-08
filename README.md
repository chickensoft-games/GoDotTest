# GoDotTest

[![Chickensoft Badge][chickensoft-badge]][chickensoft-website] [![Discord][discord-badge]][discord] [![Read the docs][read-the-docs-badge]][docs] ![line coverage][line-coverage] ![branch coverage][branch-coverage]

C# test runner for Godot. Run tests from the command line, collect code coverage, and debug tests in VSCode.

---

<p align="center">
<img alt="Chickensoft.GoDotTest" src="Chickensoft.GoDotTest/icon.png" width="200">
</p>

## Installation

Find the latest version of [GoDotTest] on nuget. GoDotTest versions that use pre-release versions of Godot have a matching prerelease label in the version name.

Add the latest version of GoDotTest to your `*.csproj` file. Make sure to replace `*VERSION*` with the latest version.

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.GoDotTest" Version="*VERSION*" />
</ItemGroup>
```

## Examples

The example below shows how unit tests are written. Each test extends the provided `TestClass` and receives the test scene as a constructor argument which is passed to the base class. The test scene can be used by tests to create nodes and add them to the scene tree.

```csharp
using Chickensoft.GoDotTest;
using Chickensoft.Log;
using Godot;

public class ExampleTest : TestClass {
  private readonly ILog _log = new Log(nameof(ExampleTest), new TraceWriter());

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

  [Failure]
  public void Failure() =>
    _log.Print("Runs whenever any of the tests in this suite fail.");
}
```

Tests can leverage lifecycle attributes to perform setup steps and/or cleanup steps. The `[Setup]` attribute is called before each test and the `[Cleanup]` attribute is called after each test.

Likewise, the `[SetupAll]` attribute is called before the first test runs, and the `[CleanupAll]` attribute is called after all the tests have run. Tests are always executed in the order that they are defined in the test class.

Any methods marked with the `Failure` attribute will be run whenever a test in the same suite fails. Failure methods can be used to take screenshots or manage errors in a specific way.

Below is the test execution output GoDoTest shows for its own tests:

![test output](docs/test_output.png)

## Setup

You can debug tests in Godot from Visual Studio Code. To do this, you will need to specify the `GODOT` environment variable for the following launch configurations and scripts to work correctly. The `GODOT` variable should point to the path of the Godot executable.

See the [Chickensoft Setup Guide][chickensoft-setup-guide] for more information about setting up your development environment for use with Godot and GoDotTest.

## Debugging (Visual Studio Code)

The following `launch.json` file provides launch configurations to debug the game, debug all the tests, or debug the currently open test in Visual Studio Code. To debug the currently open test, make sure the class name of the test matches the file name, as is typical in C#.

### Godot 4 Launch Configurations

Place the following `tasks.json` and `launch.json` inside a folder named `.vscode` in the root of your project. If you open your project from the root from within VSCode, you will be able to debug your game and its tests.

#### tasks.json

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

#### launch.json

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

## Debugging (Visual Studio)

To debug your tests from Visual Studio, place the following `launchSettings.json` file in the `Properties` subdirectory of your test project:

```json
{
  "profiles": {
    "Debug Tests": {
      "commandName": "Executable",
      "executablePath": "%GODOT4%",
      "commandLineArgs": "--run-tests --listen-trace --quit-on-finish",
      "workingDirectory": "."
    }
  }
}
```

Alternatively, [edit the launch profiles for your test project](https://learn.microsoft.com/en-us/visualstudio/debugger/project-settings-for-csharp-debug-configurations-dotnetcore?view=vs-2022) in Visual Studio. Add an executable profile with these settings:

![vs launch config](docs/vs_launch_config.png)

## Testing a Scene

Create a `test` folder in your project and create a test scene in it. Add a C# script to the root of the test scene with the following contents:

```csharp
using System.Reflection;
using Godot;
using GoDotTest;

public partial class Tests : Node2D {
  public override async void _Ready()
    => await GoTest.RunTests(Assembly.GetExecutingAssembly(), this);
}
```

## Main Scene

How you utilize GoDotTest will vary based on whether you are creating a game or a nuget package for use with Godot and C#.

### Games

In your main scene, you can tell GoDotTest to look at the command line arguments given to the Godot process and construct a test environment object that can be used to determine if tests should be run.

If tests need to be run, you can instruct GoDotTest to find and execute tests in the current assembly.

Because you typically do not want to include tests in release builds of your game, you can exclude all of the test files from the build by adding the following to your `.csproj` file (change `test/**/*` to the relative path of your test files within the project if they are not in a folder at the root called `test`):

```xml
<PropertyGroup>
  <DefaultItemExcludes Condition="'$(Configuration)' == 'ExportRelease'">
    $(DefaultItemExcludes);test/**/*
  </DefaultItemExcludes>
</PropertyGroup>
```

Add the following script to the main scene (the entry point) of your Godot game. If you already have a customized main scene, rename it to `Game.tscn` and make a new main scene that is completely empty. If you are making a 3D game, make the root a Node3D instead of a Node2D.

Note that this script relies on your game's actual beginning scene to be `Game.tscn`: if you don't have one, you'll need to create one. If tests do not need to be run, your game will start and immediately switch to `Game.tscn`. Otherwise, the main scene will ask GoDotTest to find and run tests in the current assembly.

```csharp
namespace YourGame;

using Godot;

#if DEBUG
using System.Reflection;
using GoDotTest;
#endif

public partial class Main : Node2D {
#if DEBUG
  public TestEnvironment Environment = default!;
#endif

  public override void _Ready() {
#if DEBUG
    // If this is a debug build, use GoDotTest to examine the
    // command line arguments and determine if we should run tests.
    Environment = TestEnvironment.From(OS.GetCmdlineArgs());
    if (Environment.ShouldRunTests) {
      CallDeferred("RunTests");
      return;
    }
#endif
    // If we don't need to run tests, we can just switch to the game scene.
    GetTree().ChangeSceneToFile("res://src/Game.tscn");
  }

#if DEBUG
  private void RunTests()
    => _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
#endif
}
```

### Packages

If you're creating a nuget package for use with Godot, you should create a separate test project that references your nuget package project.

Inside your test project, create a main scene and add the following script to it.

```csharp
namespace MyProject.Tests;

using System.Reflection;
using Godot;
using GoDotTest;

public partial class Tests : Node2D {
  public override void _Ready()
    => _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this);
}
```

For best results, consider using the `dotnet new` [GodotPackage] template by Chickensoft to quickly create a new Godot C# package project that is already setup to work with GoDotTest.

## Logging

If you have trouble seeing test logs, try increasing the `Max Chars per Second`, `Max Queued Messages`, `Max Errors per Second`, and `Max Warnings per Second` in the Network Limits of your project's settings (you may need to toggle Advanced Settings on to see them).

## Assertions and Mocking

GoDotTest is only a test provider and test execution system. Keeping the scope of GoDotTest small allows us to update it rapidly and ensure it's always working well with the latest Godot versions.

For mocking, we recommend [LightMock.Generator]. It is similar in usage to the popular `Moq` library, but generates mocks at compile-time ensuring maximum compatibility in any .NET environment. To make LightMock's API even more closely resemble Moq's, you can use Chickensoft's [LightMoq] adapter.

For integration tests, we recommend [GodotTestDriver](https://github.com/chickensoft-games/GodotTestDriver). GodotTestDriver allows you to create drivers that allow you to simulate input, wait for the next frame, interact with UI elements, create custom test drivers, etc.

## Coverage

If your code is configured correctly to switch to the test scene when `--run-tests` is passed in (see above), you can use [coverlet] tool to run Godot and collect code coverage from your tests.

![test coverage](docs/test_coverage.png)

```sh
coverlet \
  "./.godot/mono/temp/bin/Debug" --verbosity detailed \
  --target $GODOT4 \
  --targetargs "--run-tests --coverage --quit-on-finish" \
  --format "opencover" \
  --output "./coverage/coverage.xml" \
  --exclude-by-file "**/test/**/*.cs" \
  --exclude-by-file "**/*Microsoft.NET.Test.Sdk.Program.cs" \
  --exclude-by-file "**/Godot.SourceGenerators/**/*.cs" \
  --exclude-assemblies-without-sources "missingall"
```

The `--run-tests`, `--coverage`, and `--quit-on-finish` flags are specific to GoDotTest — they mean nothing to Godot itself. If your main scene is configured to utilize GoDotTest correctly as shown above, you can expect the [coverlet] tool to invoke Godot with the correct arguments to begin testing.

Because setting up test coverage requires a carefully constructed project, we recommend checking out the Chickensoft [GodotPackage section on collecting coverage][TestCoverage] and looking at the included `coverage.sh` script in that project.

> The `--coverage` flag tells GoDotTest that the Godot process is being executed with the intent to collect coverage. When the `--coverage` flag is supplied, GoDotTest will force-exit the process in such a way that allows it to set the exit code for the entire process, since Godot's `SceneTree.Quit(int exitCode)` method does not actually set the exit code. Force-exiting from .NET by bypassing Godot causes a few error messages to appear as the process exits, but it does not cause any other problems and can be safely disregarded.

## How It Works

The GoDotTest `TestProvider` uses reflection to find all test suites (classes that extend the provided `TestClass`) in the current assembly. If a test environment is not given to GoDotTest, it constructs its own `TestEnvironment` that represents the command line arguments given to Godot when it started and filters the test suites based on the presence of a test suite name, if given. Otherwise, it will run all test suites.

GoDotTest uses a `TestExecutor` to run methods in the order they are declared in a `TestClass`. Test methods are denoted with the `[Test]` attribute.

Test output is displayed by a `TestReporter` which responds to test events.

GoDotTest will `await` any `async Task` test methods it encounters. Tests do not run in parallel, nor are there any plans to add that functionality as that would cause race conditions when writing visual or integration-style tests. The focus of GoDotTest is to provide a reliable, C#-first approach to testing in Godot that runs tests in a very simple and deterministic manner.

If you need to customize how tests are loaded and run, you can use the code in [`GoTest.cs`](Chickensoft.GoDotTest/src/GoTest.cs) as a starting point.

## Command Line Arguments

- `--run-tests`: The presence of this flag informs your game that tests should be run. If you've setup your main scene to redirect to the test scene when it finds this flag (as described above), you can use pass this flag in when running Godot from the command line (for debugging or CI/CD purposes) to run your test(s).
- `--listen-trace`: Test output is sent to the console via Godot's capture of .NET `Trace` messages. In the Godot editor and VS Code, test output is visible through redirection of the console to a pane in the editor. However, Visual Studio does not redirect console output to its Output pane while running or debugging programs. This flag indicates that Visual Studio output should be supported by adding a `DefaultTraceListener` to the global list of trace listeners. (Using this flag in VS Code, however, will result in doubled output, as it will capture both the `DefaultTraceListener` output and the console output.)
- `--quit-on-finish`: The presence of this flag indicates that the test runner should exit the application as soon as it is finished running tests.
- `--stop-on-error`: The presence of this flag indicates that the test runner should stop running tests when it encounters the first error in any test suite. Without this flag, it will attempt to run all of the test suites.
- `--sequential`: The presence of this flag indicates that subsequent test methods in a test suite should be skipped if an error occurs in a test suite method. Use this if your test methods rely on the previous test method completing successfully. This flag is ignored when using `--stop-on-error`.
- `--coverage`: Required when running tests with the intent to collect coverage in Godot 4. Allows GoDotTest to force-exit so that coverlet picks up on the coverage correctly.

For more information about command line flags, see [`TestEnvironment.cs`](src/TestEnvironment.cs).

---

🐣 Package generated from a 🐤 Chickensoft Template — <https://chickensoft.games>

[chickensoft-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/chickensoft_badge.svg
[chickensoft-website]: https://chickensoft.games
[discord-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/discord_badge.svg
[discord]: https://discord.gg/gSjaPgMmYW
[read-the-docs-badge]: https://raw.githubusercontent.com/chickensoft-games/chickensoft_site/main/static/img/badges/read_the_docs_badge.svg
[docs]: https://chickensoft.games/docs
[line-coverage]: Chickensoft.GoDotTest.Tests/badges/line_coverage.svg
[branch-coverage]: Chickensoft.GoDotTest.Tests/badges/branch_coverage.svg

[GoDotTest]: https://github.com/chickensoft-games/GoDotTest
[GodotPackage]: https://github.com/chickensoft-games/GodotPackage
[chickensoft-setup-guide]: https://chickensoft.games/docs/setup
[LightMoq]: https://github.com/chickensoft-games/LightMoq
[LightMock.Generator]: https://github.com/anton-yashin/LightMock.Generator
[TestCoverage]: https://github.com/chickensoft-games/GodotPackage#-test-coverage
[GodotTestDriver]: https://github.com/derkork/godot-test-driver
[coverlet]: https://github.com/coverlet-coverage/coverlet
