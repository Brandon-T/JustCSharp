using System;
using System.Collections.Generic;

public class Validator
{
    public Validator() { }

    public bool Equals<T>(T First, T Second)
    {
        return EqualityComparer<T>.Equals(First, Second);
    }
}