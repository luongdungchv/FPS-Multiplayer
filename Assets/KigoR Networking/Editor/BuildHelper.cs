using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEditor.Build;
using System;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.Linq;

public class BuildHelper : Editor
{
    [MenuItem("Build Helper/Build Server")]

    private static void BuildServer()
    {
        var oldDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Server);
        var splitOldDefines = oldDefines.Split(';');
        var newDefines = new List<string>(splitOldDefines);
        for (int i = 0; i < newDefines.Count; i++)
        {
            var define = newDefines[i];
            if (define == "CLIENT_BUILD") newDefines[i] = "SERVER_BUILD";
        }
        if(!newDefines.Contains("SERVER_BUILD")) newDefines.Add("SERVER_BUILD");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Server, newDefines.ToArray());

        var buildOptions = new BuildPlayerOptions();

        var scenesInBuild = EditorBuildSettings.scenes;
        var scenePaths = new List<string>();
        foreach (var scene in scenesInBuild)
        {
            if (scene.enabled)
            {
                scenePaths.Add(scene.path);
            }
        }
        buildOptions.scenes = scenePaths.ToArray();
        buildOptions.target = BuildTarget.StandaloneWindows;
        buildOptions.subtarget = (int)StandaloneBuildSubtarget.Server;

        buildOptions.locationPathName = "./Builds/Server/Server.exe";

        AssetDatabase.Refresh();

        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + report.summary.totalSize + " bytes");
        }
        else if (report.summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
        }

        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Server, splitOldDefines);
    }

    [MenuItem("Build Helper/Build Client")]
    private static void BuildClient()
    {
        var oldDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
        var splitOldDefines = oldDefines.Split(';');
        var newDefines = new List<string>(splitOldDefines);
        
        for (int i = 0; i < newDefines.Count; i++)
        {
            var define = newDefines[i];
            if (define == "SERVER_BUILD") newDefines[i] = "CLIENT_BUILD";
        }
        if(!newDefines.Contains("CLIENT_BUILD")) newDefines.Add("CLIENT_BUILD");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Server, newDefines.ToArray());

        var buildOptions = new BuildPlayerOptions();

        var scenesInBuild = EditorBuildSettings.scenes;
        var scenePaths = new List<string>();
        foreach (var scene in scenesInBuild)
        {
            if (scene.enabled)
            {
                scenePaths.Add(scene.path);
            }
        }
        buildOptions.scenes = scenePaths.ToArray();
        buildOptions.target = BuildTarget.StandaloneWindows;
        buildOptions.subtarget = (int)StandaloneBuildSubtarget.Player;

        buildOptions.locationPathName = "./Builds/Client/Client.exe";

        AssetDatabase.Refresh();
        
        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + report.summary.totalSize + " bytes");
        }
        else if (report.summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
        }

        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, splitOldDefines);
    }


    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorSceneManager.sceneOpening -= SceneChangeCallback;
        EditorSceneManager.sceneOpening += SceneChangeCallback;
    }

    private static void SceneChangeCallback(string path, UnityEditor.SceneManagement.OpenSceneMode mode)
    {
        var sceneName = "";
        for (int i = path.LastIndexOf("/") + 1; i < path.Length; i++)
        {
            if (path[i] == '.') break;
            sceneName += path[i].ToString();
        }
        var seperatorIndex = sceneName.IndexOf("_");
        if (seperatorIndex < 0) return;

        var prefix = sceneName.Substring(0, seperatorIndex);
        if (prefix == "Client")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneWindows);
            Debug.Log("client");
            var oldDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            var splitOldDefines = oldDefines.Split(';');
            var newDefines = new string[splitOldDefines.Length];
            Array.Copy(splitOldDefines, newDefines, newDefines.Length);
            for (int i = 0; i < newDefines.Length; i++)
            {
                var define = newDefines[i];
                if (define == "SERVER_BUILD") newDefines[i] = "CLIENT_BUILD";
            }
            var definesList = newDefines.ToList();
            if (!definesList.Contains("CLIENT_BUILD")) definesList.Add("CLIENT_BUILD");

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, definesList.ToArray());
        }
        if (prefix == "Server")
        {
            Debug.Log("server");
            EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, BuildTarget.StandaloneWindows);

            var oldDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Server);
            var splitOldDefines = oldDefines.Split(';');
            var newDefines = new string[splitOldDefines.Length];
            Array.Copy(splitOldDefines, newDefines, newDefines.Length);
            for (int i = 0; i < newDefines.Length; i++)
            {
                var define = newDefines[i];
                if (define == "CLIENT_BUILD") newDefines[i] = "SERVER_BUILD";
            }
            var definesList = newDefines.ToList();
            if (!definesList.Contains("SERVER_BUILD")) definesList.Add("SERVER_BUILD");

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Server, definesList.ToArray());
        }
    }
}
