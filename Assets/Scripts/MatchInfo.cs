using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchInfo{
	public List<GridItem> match;
	public int matchStartingX;
	public int matchStartingY;
	public int matchEndingX;
	public int matchEndingY;

	public bool validMatch
	{
		get{return match != null;}
	}
}
