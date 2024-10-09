using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReader : IInputReader
{
    public Vector2 InputVector{
        get{
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");

            return new Vector2(x, y);
        }
    }

    public int MouseHold => throw new System.NotImplementedException();

    public int MouseDown => throw new System.NotImplementedException();

    public int MouseUp => throw new System.NotImplementedException();
}
