# Lethal TerminalExtender
A modding utility for Lethal Company that allows mod to adds their own custom commands to the ship terminal.

![terminalextender](https://github.com/domkalan/lethalterminalextender/raw/main/images/example1.gif)

## Some Notes
* Mod is entirely client side and does not attempt to handle any networking logic, this should be handled by your mod instead.
* A custom interaction mode can be enabled that will temporarily disable the default terminal commands.

## Todo
- [ ] Need to consolidate harmony patches into a single class.
- [ ] Add an event/callback for when user begins/ends interaction with terminal.

## Getting Started
Download the current built .dll file in the releases and include it as a refrence in your local mod project. When packaging your mode, be sure to either include it (local play) or include it as a dependency (mod manager).

Once included in your project, inside your mod you can run the following to add a custom command.
```C#
TerminalExtenderUtils.addQuickCommand("helloworld", "Hello World!", true, (Terminal term, TerminalNode node) =>
{
    // a callback that is called when the command is ran
});
```

If you need/want to define your own keyword, node, and compatible nouns, you can add a command this way as well.
```c#
TerminalNode nodeObject = ScriptableObject.CreateInstance<TerminalNode>();
nodeObject.clearPreviousText = true;
nodeObject.terminalEvent = "helloWorldEvent";
nodeObject.displayText = "Hello world!";

TerminalKeyword keywordObject = ScriptableObject.CreateInstance<TerminalKeyword>();
keywordObject.word = "helloworld";
keywordObject.specialKeywordResult = nodeObject;

Action<Terminal, TerminalNode> callbackAction = (term, node) =>
{
    // run some function here
};

TerminalExtenderUtils.addCustomCommand(new TerminalCustomCommand()
{
    keyword = keywordObject,
    node = nodeObject,
    callback = callbackAction
});
```

## External Dependencies Used
* [BepinEX](https://docs.bepinex.dev/index.html)
* [Harmony 2](https://harmony.pardeike.net/)
* [LethalAPI.GamesLib](https://github.com/dhkatz/LethalAPI.GameLibs)
    * Can also be downloaded from [NuGet](https://www.nuget.org/packages/LethalAPI.GameLibs)

## New to Lethal Company Modding?
Check out this [Lethal Company Modding Wiki](https://lethal.wiki/) which has some good resources on how modding the game works, and even guides to add multiplayer to your mod.

## License
Feel free to fork, submit pull requests, or use this mod in any way shape or form as long as you include the LICENSE file which is above in the repo. The source uses the open source MIT License.