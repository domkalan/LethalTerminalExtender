using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace LethalTerminalExtender.Patches
{
    public struct TerminalCustomCommand
    {
        public TerminalNode node;
        public TerminalKeyword keyword;
        
        [CanBeNull]
        public Action<Terminal, TerminalNode> callback;
    }
    
    /**
     * TerminalExtenderUtils
     *
     * Allows easy and reusable extension of the in game terminal.
     */
    public static class TerminalExtenderUtils
    {
        private static List<TerminalCustomCommand> commands = new List<TerminalCustomCommand>();
        
        private static bool terminalSynced = false;
        private static Terminal terminalInstance = null;

        private static Action<string> currentStapleCommand = null;

        private static bool stapleNoticeShown = false;

        public static bool terminalConnected
        {
            get
            {
                return terminalSynced;
            }
            set
            {
                throw new Exception("Can not set this!");
            }
        }
        public static bool currentStapleCommandPresent
        {
            get
            {
                return currentStapleCommand != null;
            }
        }

        private static TerminalNode blankNodeInstance;
        private static TerminalNode customTextNodeInstance;
        
        public static void addCustomCommand(TerminalCustomCommand custComm)
        {
            commands.Add(custComm);
        }
        
        
        public static TerminalCustomCommand addQuickCommand(string keyword, string displayText, bool clearsText = false, Action<Terminal, TerminalNode> callback = null)
        {
            TerminalNode nodeCommand = ScriptableObject.CreateInstance<TerminalNode>();
            nodeCommand.name = keyword + "Node";
            nodeCommand.displayText = displayText;
            nodeCommand.clearPreviousText = clearsText;
            nodeCommand.terminalEvent = keyword + "Event";
            
            TerminalKeyword keywordCommand = ScriptableObject.CreateInstance<TerminalKeyword>();
            keywordCommand.name = keyword + "Keyword";
            keywordCommand.word = keyword;
            keywordCommand.specialKeywordResult = nodeCommand;

            TerminalCustomCommand createdCommand = new TerminalCustomCommand()
            {
                keyword = keywordCommand,
                node = nodeCommand,
                callback = callback
            };
            
            addCustomCommand(createdCommand);

            return createdCommand;
        }
        
        public static void setOnSubmit(Action<string> action)
        {
            if (!stapleNoticeShown)
            {
                stapleNoticeShown = true;
                
                HUDManager.Instance.DisplayTip("TERMINAL EXTENDER", "This command is from a mod, it may change the default functionality of the terminal.");
            }
            
            currentStapleCommand = action;
        }

        public static void releaseOnSubmit()
        {
            currentStapleCommand = null;
        }

        public static void emitOnSubmit(string text)
        {
            currentStapleCommand(text);
        }

        public static void setInputState(bool inputToggle)
        {
            if (inputToggle)
            {
                terminalInstance.screenText.interactable = true;
                terminalInstance.screenText.ActivateInputField();
                terminalInstance.screenText.Select();
            }

            if (!inputToggle)
            {
                terminalInstance.screenText.interactable = false;
                
                terminalInstance.screenText.DeactivateInputField();
            }
        }
        
        
        public static void playAudioClip(int clipIndex)
        {
            terminalInstance.PlayTerminalAudioServerRpc(clipIndex);
        }

        public static void cleanTerminalScreen()
        {
            terminalInstance.LoadNewNode(blankNodeInstance);
        }

        public static void writeToTerminal(string text, bool cleanScreen = false)
        {
            Plugin.instance.Log(LogType.Log, text + "\n\n is " + text.Length);
            
            customTextNodeInstance.clearPreviousText = cleanScreen;
            customTextNodeInstance.displayText = "\n" + text;
            
            terminalInstance.LoadNewNode(customTextNodeInstance);
        }

        public static void displayNodeOnTerminal(TerminalNode node)
        {
            terminalInstance.LoadNewNode(node);
        }

        public static bool handleNodeEvents(TerminalNode node)
        {
            TerminalCustomCommand[] currentCommands = commands.ToArray();
            
            foreach (TerminalCustomCommand command in currentCommands)
            {
                if (command.node == node)
                {
                    if (command.callback != null)
                    {
                        command.callback(terminalInstance, node);
                    
                        return false;
                    }

                    break;
                }
            }

            return true;
        }

        public static TerminalKeyword[] getKeywords()
        {
            TerminalKeyword[] keywords = new TerminalKeyword[commands.Count];
            
            for(int i = 0; i < commands.Count; i++)
            {
                keywords[i] = commands[i].keyword;
            }
            
            return keywords;
        }
        public static TerminalKeyword handleKeywordCommand(string typedKeyword)
        {
            foreach (TerminalKeyword keyword in TerminalExtenderUtils.getKeywords())
            {
                if (keyword.word == typedKeyword)
                {
                    return keyword;
                }
            }

            return null;
        }

        public static bool handleTerminalSubmit()
        {
            if (currentStapleCommandPresent)
            {
                Plugin.instance.Log(LogType.Log, "Staple mode is active!");
                
                string currentText = terminalInstance.RemovePunctuation(terminalInstance.screenText.text.Substring(terminalInstance.screenText.text.Length - terminalInstance.textAdded));
                
                Plugin.instance.Log(LogType.Log, "Request to parse command in staple mode: " + currentText);
                
                TerminalExtenderUtils.emitOnSubmit(currentText);
                
                return false;
            }
            
            return true;
        }

        public static void handleTerminalQuit()
        {
            if (currentStapleCommandPresent)
            {
                releaseOnSubmit();
            }
        }
        

        public static void connectToTerminal(Terminal terminal)
        {
            if (terminalSynced)
                return;

            if (terminalInstance == null)
                terminalInstance = terminal;

            // Create our blank node for when the terminal needs to be empty
            TerminalNode blankNodeObj = ScriptableObject.CreateInstance<TerminalNode>();
            blankNodeObj.name = "blankNode";
            blankNodeObj.displayText = "";
            blankNodeObj.clearPreviousText = true;

            // set the static property
            blankNodeInstance = blankNodeObj;
            
            // add it to our node list
            terminalInstance.terminalNodes.terminalNodes.Add(blankNodeObj);
            
            // Create a node for custom text to be displayed on screen
            TerminalNode textNodeObj = ScriptableObject.CreateInstance<TerminalNode>();
            textNodeObj.name = "textNode";
            textNodeObj.displayText = "Hello world!";
            textNodeObj.clearPreviousText = false;

            // set the static property
            customTextNodeInstance = textNodeObj;
            
            // add it to our node list
            terminalInstance.terminalNodes.terminalNodes.Add(textNodeObj);

            // Inject all of our commands into the terminal
            foreach (TerminalCustomCommand command in commands)
            {
                if (!terminalInstance.terminalNodes.terminalNodes.Contains(command.node))
                {
                    terminalInstance.terminalNodes.terminalNodes.Add(command.node);
                }

                if (terminalInstance.terminalNodes.allKeywords.Contains(command.keyword))
                {
                    terminalInstance.terminalNodes.allKeywords.AddToArray(command.keyword);
                }
                    
            }
            
            // The terminal has been synced, this prevent this from running again
            terminalSynced = true;
        }
    }
    
    /*
     * SECTION: Patches
     */
    
    /*
     * HandleNodeEvent
     * Handles all node events emitted by the terminal.
     */
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.RunTerminalEvents))]
    public class TerminalExtender_HandleNodeEvent
    {
        public static bool Prefix(Terminal __instance, TerminalNode node)
        {
            // Make sure we are connected
            if (!TerminalExtenderUtils.terminalConnected)
                TerminalExtenderUtils.connectToTerminal(__instance);
            
            return TerminalExtenderUtils.handleNodeEvents(node);
        }
    }
    
    /*
     * KeywordCheck
     *
     * Checks to see if our keyword is called.
     */
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.CheckForExactSentences))]
    public class TerminalExtender_KeywordCheck
    {
        public static void Postfix(Terminal __instance, string playerWord, ref TerminalKeyword __result)
        {
            // Make sure we are connected
            if (!TerminalExtenderUtils.terminalConnected)
                TerminalExtenderUtils.connectToTerminal(__instance);
            
            if (__result == null)
            {
                Plugin.instance.Log(LogType.Log, "request to parse command " + playerWord);

                __result = TerminalExtenderUtils.handleKeywordCommand(playerWord);
            }
        }
    }

    /*
     * OnSubmit
     *
     * Allows for custom prompt interactions from the terminal by taking over the onsubmit on the terminal.
     */
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.OnSubmit))]
    public class TerminalExtender_OnSubmit
    {
        public static bool Prefix(Terminal __instance)
        {
            // Make sure we are connected
            if (!TerminalExtenderUtils.terminalConnected)
                TerminalExtenderUtils.connectToTerminal(__instance);

            return TerminalExtenderUtils.handleTerminalSubmit();
        }
    }

    /**
     * QuitTerminal
     *
     * Notify when the player leaves the terminal, very important.
     */
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.QuitTerminal))]
    public class TerminalExtender_QuitTerminal
    {
        public static void Prefix(Terminal __instance)
        {
            // Make sure we are connected
            if (!TerminalExtenderUtils.terminalConnected)
                TerminalExtenderUtils.connectToTerminal(__instance);
            
            TerminalExtenderUtils.handleTerminalQuit();
        }
    }
}