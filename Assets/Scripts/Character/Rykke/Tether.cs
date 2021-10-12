﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tether : MonoBehaviour
{

	protected static float framerateScalar =.0833333333f;
	private Vector2 target = new Vector2(.2f,.16f);
	private Vector2 targetOffset = new Vector2(.2f,0);
	private float speed = 2f;
	Collider2D hitbox;

	float extendPercent = 0;
	SpriteRenderer sprite;
    // Start is called before the first frame update
    void Start()
    {
    	sprite = GetComponent<SpriteRenderer>();   
    	hitbox = GetComponentInChildren<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        sprite.size = Vector2.MoveTowards(sprite.size,target,extendPercent);
        hitbox.offset = Vector2.MoveTowards(hitbox.offset,targetOffset,extendPercent);

        if(extendPercent < 1f) extendPercent+= Time.deltaTime/(speed * framerateScalar);
    }

    public void setTargetLength(float len)
    {
    	extendPercent = 0;
    	target = new Vector2(len,.16f);
    	targetOffset = new Vector2(len,0);
    }

    public void setSpeed(float spd)
    {
    	speed = spd;
    }

    public void freezeLen()
    {
    	target = sprite.size;
    	targetOffset = hitbox.offset;
    	extendPercent = 1;
    }

    public void togglehitbox(int active)
    {
    	hitbox.enabled = (active != 0);
    }
}