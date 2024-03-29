﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine
{
    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
    /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">minValue is greater than maxValue.</exception>
    public delegate int RandomGenerator(int minValue, int maxValue);

}
