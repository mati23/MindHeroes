using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random=UnityEngine.Random;
public class GameGrid : MonoBehaviour {

	public int xSize,ySize;
	public float heroSpriteWidth=1f;
	private GameObject[] _heroes;
	private GridItem[,] _items;
	private GridItem _currentSelectedItem;
	public static int minItemsForMatch=3;
    public float delayBetweenMatches = 0.2f;
	// Use this for initialization
	void Start ()
    {
		GetHeroes();
		FillGrid();
        ClearGrid();
		GridItem.OnMouseOverItemEventHandler+=OnMouseOverItem;	
		
	}

	void OnDisable(){
		 GridItem.OnMouseOverItemEventHandler-=OnMouseOverItem;
	 }
	
	void FillGrid(){
		_items = new GridItem [xSize,ySize];

		for(int x=0; x<xSize;x++){
			for(int y=0;y<ySize;y++){
                _items[x, y] = InstantiateHero(x, y);
            
			}
		}
	}
	 

	GridItem InstantiateHero(int x, int y){
		GameObject randomHero = _heroes[Random.Range(0,_heroes.Length)];
		GridItem newHero = ((GameObject) Instantiate (randomHero, new Vector3(x * heroSpriteWidth,y),Quaternion.identity)).GetComponent<GridItem>();
		newHero.OnItemPositionChanged(x,y);
		return newHero;
	}

	void OnMouseOverItem(GridItem item){
		if(_currentSelectedItem==item){
			return;
		}
		if(_currentSelectedItem == null){
			_currentSelectedItem=item;
		}else{

			float xDiff = Math.Abs(item.x - _currentSelectedItem.x);
			float yDiff = Math.Abs(item.y - _currentSelectedItem.y);
			if(xDiff+yDiff == 1){
				StartCoroutine(TryMatch (_currentSelectedItem, item));
				
				}else{
					Debug.LogError("LONGE DEMAIS");
				}
			_currentSelectedItem=null;
		}
	}

	IEnumerator TryMatch(GridItem a, GridItem b){
        yield return StartCoroutine(Swap(a, b));
        MatchInfo matchA = GetMatchInformation(a);
		MatchInfo matchB = GetMatchInformation(b);
		
		if(!matchA.validMatch && !matchB.validMatch){
			yield return StartCoroutine (Swap(a,b));
			yield break;
		}
        if (matchA.validMatch)
        {
            yield return StartCoroutine(DestroyItems(matchA.match));
            yield return new WaitForSeconds(delayBetweenMatches);
            yield return StartCoroutine(UpdateGridAfterMatch(matchA));
        }
        else if (matchB.validMatch)
        {
            yield return StartCoroutine(DestroyItems(matchB.match));
            yield return new WaitForSeconds(delayBetweenMatches);
            yield return StartCoroutine(UpdateGridAfterMatch(matchB));
        }
	}

    IEnumerator UpdateGridAfterMatch(MatchInfo match) 
    {
        if (match.matchStartingY == match.matchEndingY)
        {
            for(int x = match.matchStartingX; x <= match.matchEndingX; x++)
            {
                for(int y = match.matchStartingY;y < ySize-1; y++)
                {
                    GridItem upperIndex = _items[x, y + 1];
                    GridItem current = _items[x, y];
                    _items[x, y] = upperIndex;
                    _items[x, y + 1] = current;
                    _items[x, y].OnItemPositionChanged(_items[x, y].x, _items[x, y].y - 1);
                }
                _items[x,ySize-1]= InstantiateHero(x, ySize - 1);
            }
        }
        if(match.matchEndingX == match.matchStartingX)
        {
            int matchHeight = 1+(match.matchEndingY - match.matchStartingY);
            for(int y =match.matchStartingY+matchHeight;y<= ySize - 1; y++)
            {
                GridItem lowerIndex = _items[match.matchStartingX, y -matchHeight];
                GridItem current = _items[match.matchStartingX, y];
                _items[match.matchStartingX, y - matchHeight]=current;
                _items[match.matchStartingX, y] = lowerIndex;

            }
            for(int y=0;y<ySize-matchHeight;y++)
            {
                _items[match.matchStartingX, y].OnItemPositionChanged(match.matchStartingX, y);
            }
            for (int i = 0; i < match.match.Count;i++)
            {
                _items[match.matchStartingX, (ySize - 1) - i] = InstantiateHero(match.matchStartingX, (ySize - 1) - i);
            }
        }

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                MatchInfo matchInfo = GetMatchInformation(_items[x, y]);
                if (matchInfo.validMatch)
                {
                    yield return new WaitForSeconds(delayBetweenMatches);
                    yield return StartCoroutine(DestroyItems(matchInfo.match));
                    yield return new WaitForSeconds(delayBetweenMatches);
                    yield return StartCoroutine(UpdateGridAfterMatch(matchInfo));
                }

            }
        }

    }

    IEnumerator DestroyItems(List<GridItem> items)
    {

        foreach(GridItem i in items)
        {
            if (i != null)
            {
                yield return StartCoroutine(i.transform.Scale(Vector3.zero, 0.01f));
                Destroy(i.gameObject);
            }
            
        }
       
    }
	
	IEnumerator Swap(GridItem a, GridItem b){
		ChangeRigidBodyStatus(false);
		float movDuration = 0.1f;
        Vector3 aPosition = a.transform.position;
		StartCoroutine (a.transform.Move(b.transform.position, movDuration));
		StartCoroutine (b.transform.Move(aPosition, movDuration));
		yield return new WaitForSeconds(movDuration);
		SwapIndices(a,b);
		ChangeRigidBodyStatus(true);
	}

	void SwapIndices(GridItem a, GridItem b){
		GridItem tempA =  _items[a.x,a.y];
		_items[a.x,a.y]=b;
		_items[b.x,b.y]=tempA;
		int bOldX = b.x; int bOldY = b.y;
		b.OnItemPositionChanged(a.x,a.y);
		a.OnItemPositionChanged(bOldX,bOldY);
	}
	

	List<GridItem> SearchHorizontally(GridItem item){
		List<GridItem> hItems = new List<GridItem>{item}; 
		int left = item.x-1;
		int right = item.x+1;
		while (left>=0 && _items[left, item.y].id==item.id)
		{
			hItems.Add(_items[left, item.y]);
			left--;
		}
		while (right< xSize && _items[right, item.y].id==item.id)
		{
			hItems.Add(_items[right, item.y]);
			right++;
		}
		return hItems;
	}

	MatchInfo  GetMatchInformation(GridItem item){
		MatchInfo m = new MatchInfo();
		m.match = null;
		List<GridItem> hMatch = SearchHorizontally(item);
		List<GridItem> vMatch = SearchVertically(item);
		if(hMatch.Count >= minItemsForMatch && hMatch.Count > vMatch.Count){
			//definir infos para match horizontal
			m.matchStartingX= GetMinimumX(hMatch);
			m.matchEndingX= GetMaximumX(hMatch);
			m.matchStartingY=m.matchEndingY = hMatch[0].y;
			m.match = hMatch;

		}else if(vMatch.Count >= minItemsForMatch){
			//define informacoes para match vertical
			m.matchStartingY= GetMinimumY(vMatch);
			m.matchEndingY= GetMaximumY(vMatch);
			m.matchStartingX = m.matchEndingX = vMatch[0].x;
			m.match = vMatch;
		}
		return m;
	}

	int GetMinimumX(List<GridItem> items){
		float[] indices = new float [items.Count];
		for (int i=0; i<indices.Length; i++){
			indices[i] = items[i].x;
		}
		return (int) Mathf.Min(indices);
	}
	int GetMaximumX(List<GridItem> items){
			float[] indices = new float [items.Count];
			for (int i=0; i<indices.Length; i++){
				indices[i] = items[i].x;
			}
			return (int) Mathf.Max(indices);
	}

	int GetMinimumY(List<GridItem> items){
		float[] indices = new float [items.Count];
		for (int i=0; i<indices.Length; i++){
			indices[i] = items[i].y;
		}
		return (int) Mathf.Min(indices);
	}
	int GetMaximumY(List<GridItem> items){
			float[] indices = new float [items.Count];
			for (int i=0; i<indices.Length; i++){
				indices[i] = items[i].y;
			}
			return (int) Mathf.Max(indices);
	}
	
	List<GridItem> SearchVertically(GridItem item){
		List<GridItem> vItems = new List<GridItem>{item};
		int lower = item.y-1;
		int upper = item.y+1;
		while (lower>= 0 && _items[item.x, lower].id == item.id)
		{
			vItems.Add(_items[item.x,lower]);
			lower--;			
		}
		while (upper< ySize && _items[item.x,upper].id==item.id)
		{
			vItems.Add(_items[item.x,upper]);
			upper++;
		}
		return vItems;
	}
	
	void GetHeroes(){
		_heroes = Resources.LoadAll<GameObject>("Prefabs");
		for(int i =0; i< _heroes.Length;i++){
			_heroes[i].GetComponent<GridItem>().id = i;
		}
	}

	void ChangeRigidBodyStatus(bool status){
		foreach (GridItem g in _items){
			g.GetComponent<Rigidbody2D>().isKinematic = !status;
		}
	}

    void ClearGrid()
     {
         for (int x = 0; x < xSize; x++)
         {
             for (int y = 0; y < ySize; y++ ){
                 MatchInfo matchInfo = GetMatchInformation(_items[x,y]);
                 if (matchInfo.validMatch)
                 {
                     Destroy(_items[x,y].gameObject);
                     _items[x,y]=InstantiateHero(x, y);
                     y--;
                 }

         }
         }
     }
}
