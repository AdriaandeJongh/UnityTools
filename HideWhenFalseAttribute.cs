// Put this script outside the Editor folder.
// By Adriaan de Jongh, http://adriaandejongh.nl
// More info & other Unity scripts: https://github.com/AdriaandeJongh/UnityTools

using UnityEngine;
using System.Collections;

public class HideWhenFalseAttribute : PropertyAttribute 
{
	public readonly string hideBoolean;

	public HideWhenFalseAttribute (string booleanName)
	{
		this.hideBoolean = booleanName;
	}
}
