using UnityEngine;
using System.Collections;

public class GUIMultiDotSlider{
	
	Texture lft;
	Texture mid;
	Texture rgt;
	Texture dot;
	
	Vector2 rlft;
	Vector2 rmid;
	Vector2 rrgt;
	Vector2 rdot;
	
	Rect grp;
	
	int nbDot=0;
	int posDot=0;
	
	Color slct = new Color(1,1,1,1f);
	Color unSlct = new Color(0,0,0,0.5f);
	Color bckgrnd = new Color(0,0,0,0.25f);
	
	float scale = 0.4f;
	
	public GUIMultiDotSlider(int nb)
	{
		nbDot = nb;
		
		lft = (Texture) Resources.Load("multiDotSlider/lft", typeof(Texture));
		mid = (Texture) Resources.Load("multiDotSlider/mid", typeof(Texture));
		rgt = (Texture) Resources.Load("multiDotSlider/rgt", typeof(Texture));
		dot = (Texture) Resources.Load("multiDotSlider/dot", typeof(Texture));
		
		
		rlft = new Vector2(21*scale,44*scale);
		rmid = new Vector2(36*scale,44*scale);
		rrgt = new Vector2(21*scale,44*scale);
		rdot = new Vector2(42*scale,44*scale);
		
		if(nb>1)
			grp = new Rect(0,0,rlft.x+((nbDot-1)*rmid.x)+rrgt.x,rlft.y);
		
		
		
	}
	
	public void setColors(Color slctC,Color unSlctC,Color backC)
	{
		slct = slctC;
		unSlct = unSlctC;
		bckgrnd = backC;
	}
	
	public void setNbDot(int nb)
	{
		nbDot = nb;
		grp = new Rect(grp.x,grp.y,rlft.x+((nbDot-1)*rmid.x)+rrgt.x,rlft.y);
	}
	
	public void setDotPos(int n)
	{
		if(n>-1 && n<nbDot)
			posDot = n;
	}
	
	public void setPos(Vector2 p)
	{
		grp.x = p.x;
		grp.y = p.y;
	}
	
	public Rect getGrp()
	{
		return grp;	
	}
		
	
	public void ui()
	{
		GUI.BeginGroup(grp);
		//affichage dots
		for(int i=0;i<nbDot;i++)
		{	
			if(i==posDot)
				GUI.color = slct;
			else
				GUI.color = unSlct;
			
			if(i==0)
			{
				GUI.DrawTexture(new Rect(0,0,rdot.x,rdot.y),dot);
			}
			else
			{
				GUI.DrawTexture(new Rect(i*rmid.x,0,rdot.x,rdot.y),dot);
			}
		}
		
		//affichage background
		for(int j=0;j<nbDot+1;j++)
		{
			GUI.color = bckgrnd;
			if(j == 0)
			{
				GUI.DrawTexture(new Rect(0,0,rlft.x,rlft.y),lft);
			}
			else if(j != nbDot)
			{
				GUI.DrawTexture(new Rect(rlft.x+(j-1)*rmid.x,0,rmid.x,rmid.y),mid);
			}
			else
			{
				GUI.DrawTexture(new Rect(rlft.x+(j-1)*rmid.x,0,rrgt.x,rrgt.y),rgt);
			}
		}
		GUI.EndGroup();
		GUI.color = Color.white;
	}
	
}
