using System;
using UnityEngine;

public class LightningBolt : MonoBehaviour
{
    public Transform target;
    public int zigs = 100;
    public float speed = 1f;
    public float scale = 1f;

    Perlin noise;
    float oneOverZigs;

    private Particle[] particles;

    ParticleEmitter particleEmitter;
    ParticleRenderer particleRenderer;

    bool playing;

    public void Start()
    {
        oneOverZigs = 1f / zigs;
        particleEmitter = GetComponent<ParticleEmitter>();
        particleRenderer = GetComponent<ParticleRenderer>();
        particleEmitter.emit = false;

        particleEmitter.Emit( zigs );
        particles = particleEmitter.particles;
        particleRenderer.enabled = false;
    }

    public void Update()
    {
        if ( !playing )
        {
            return;
        }

        if ( noise == null )
            noise = new Perlin();

        var timex = Time.time * speed * 0.1365143f;
        var timey = Time.time * speed * 1.21688f;
        var timez = Time.time * speed * 2.5564f;

        for ( int i = 0; i < particles.Length; i++ )
        {
            var position = Vector3.Lerp( transform.position, target.position, oneOverZigs * i );
            var offset = new Vector3( noise.Noise( timex + position.x, timex + position.y, timex + position.z ),
                noise.Noise( timey + position.x, timey + position.y, timey + position.z ),
                noise.Noise( timez + position.x, timez + position.y, timez + position.z ) );
            position += offset * scale * ( i * oneOverZigs );

            particles[i].position = position;
            particles[i].color = Color.white;
            particles[i].energy = 1f;
        }

        particleEmitter.particles = particles;
    }

    public void Stop()
    {
        if ( particleEmitter != null )
        {
            particleEmitter.enabled = false;
            particleEmitter.emit = false;
        }
        if ( particleRenderer != null )
        {
            particleRenderer.enabled = false;
        }
        playing = false;
    }

    public void Play( Tile focusedTile )
    {
        if ( focusedTile != null )
        {
            target = focusedTile.gameObject.transform;
            playing = true;
            particleRenderer.enabled = true;
        }
        else
        {
            Debug.LogError( "Playing SFX on unkown tile" );
        }
    }
}