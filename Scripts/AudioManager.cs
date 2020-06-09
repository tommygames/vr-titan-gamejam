using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple audio manager intended for 1 background music track and 1 SFX channel.
/// Feel free to add more audio channels, just note that they will need to be managed appropriately.
///
/// This class should be attached to the AudioManager prefab.
/// The AudioManager prefab should live somewhere in the scene.
/// </summary>
public class AudioManager : MonoBehaviour
{
	// Add more AudioSources to play multiple clips concurrently
	public AudioSource FXSource;
	public AudioSource MusicSource;
	
	public static AudioManager Instance { get; private set; }

	public float lowPitchRange = 0.95f;
	public float highPitchRange = 1.05f;

	// Use this for initialization
	void Awake()
	{
		if( Instance == null )
		{
			Instance = this;
		}
	}

	public void PlaySingle( AudioClip clip )
	{
		FXSource.clip = clip;
		FXSource.Play();
	}
	
	public void PlaySingle( string clipName )
	{
		AudioClip clip;
		if( !clipCache.TryGetValue( clipName, out clip ) )
		{
			clip = (AudioClip)Resources.Load( clipName );
			clipCache[ clipName ] = clip;
		}
		
		FXSource.clip = clip;
		FXSource.Play();
	}
	
	//RandomizeSfx chooses randomly between various audio clips and slightly changes their pitch.
	public void RandomizeSfx( params AudioClip[] clips )
	{
		int idx = Random.Range( 0, clips.Length );
		float randomPitch = Random.Range(lowPitchRange, highPitchRange);
        
		FXSource.pitch = randomPitch;
		FXSource.clip = clips[ idx ];
		FXSource.Play();
	}
	
	public void RandomizeSfx( params string[] clipNames )
	{
		int idx = Random.Range( 0, clipNames.Length );
		float randomPitch = Random.Range(lowPitchRange, highPitchRange);
		string clipName = clipNames[ idx ];

		AudioClip clip;
		if( !clipCache.TryGetValue( clipName, out clip ) )
		{
			clip = (AudioClip)Resources.Load( clipName );
			clipCache[ clipName ] = clip;
		}
        
		FXSource.pitch = randomPitch;
		FXSource.clip = clip;
		FXSource.Play();
	}
	
	private static Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
}
