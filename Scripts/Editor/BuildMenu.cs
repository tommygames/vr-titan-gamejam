using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildMenu : MonoBehaviour
{
	public static void Build(BuildPlayerOptions buildPlayerOptions)
	{
		var buildScenes = EditorBuildSettings.scenes;
		string[] scenes = new string[buildScenes.Length];
		for (int i = 0; i < buildScenes.Length; i++)
		{
			scenes[i] = buildScenes[i].path;
		}
	
		buildPlayerOptions.scenes = scenes;
		buildPlayerOptions.options = BuildOptions.None;

		EditorUserBuildSettings.SwitchActiveBuildTarget(buildPlayerOptions.target);
		BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
		BuildSummary summary = report.summary;

		if (summary.result == BuildResult.Succeeded)
		{
			Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
		}

		if (summary.result == BuildResult.Failed)
		{
			Debug.LogError("Build failed");
		}
	}

	[MenuItem("Build/Build OSX")]
	public static void BuildOSX()
	{
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.locationPathName = "build/osx/osx.app";
		buildPlayerOptions.target = BuildTarget.StandaloneOSX;

		Build(buildPlayerOptions);
	}
	
	[MenuItem("Build/Build iOS")]
	public static void BuildIOS()
	{
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.locationPathName = "build/ios";
		buildPlayerOptions.target = BuildTarget.iOS;

		Build(buildPlayerOptions);
	}

	[MenuItem("Build/Build Android")]
	public static void BuildAndroid()
	{
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.locationPathName = "build/android/android.apk";
		buildPlayerOptions.target = BuildTarget.Android;

		Build(buildPlayerOptions);
	}
}
