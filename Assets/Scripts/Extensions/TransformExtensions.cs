﻿using System.Collections;
using UnityEngine;

public static class TransformExtentions
 {
	public static IEnumerator Move(this Transform t, Vector3 target, float duration){
		Vector3 diffVector = (target - t.position);
		float diffLength = diffVector.magnitude; 
		diffVector.Normalize();
		float counter = 0;
		while (counter<duration)
		{
			float movAmount = (Time.deltaTime * diffLength)/duration;
			t.position += diffVector*movAmount;
			counter+=Time.deltaTime;
			yield return null;
		}
		t.position=target; 
	}
	
    public static IEnumerator Scale(this Transform t, Vector3 target, float duration)
    {
        Vector3 diffVector = (target - t.localScale);
        float diffLenght = diffVector.magnitude;
        diffVector.Normalize();
        float counter = 0;
        while (counter < duration)
        {
            float movAmount = (Time.deltaTime * diffLenght) / duration;
            t.localScale += diffVector * movAmount;
            counter += Time.deltaTime;
            yield return null;

        }
        t.localScale = target;
    }
}
