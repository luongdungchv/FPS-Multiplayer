using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputReader
{
   public Vector2 InputVector{get;}
   public int MouseHold{get;}
   public int MouseDown{get;}
   public int MouseUp{get;}
}
