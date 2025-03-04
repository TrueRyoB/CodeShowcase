using UnityEngine;

namespace Fujin.Constants
{
    public static class Number
    {
        private const float Threshold = 0.001f;

        public static bool IsEqualFloat(float x, float y)
        {
            return Mathf.Abs(x - y) < Threshold;
        }

        public static string GetIntAtFixedLength(int value, int length)
        {
            string result = value.ToString();
            
            // Set the largest value to the possible extent
            if (result.Length > length)
            {
                result = "9".PadRight(length - 1, '9');
            }
            
            // Pad with zeros otherwise
            else if (result.Length < length)
            {
                result = result.PadLeft(length, '0');
            }

            return result;
        }

        /// <summary>
        /// Insert comma every 3 digit and return the crafted string
        /// </summary>
        /// <param name="value"></param>
        /// /// <param name="length"></param>
        /// <param name="stepDigit"></param>
        /// <returns></returns>
        public static string InsertCommaEveryThreeDigits(int value, int length)
        {
            string result = GetIntAtFixedLength(value, length);
            
            int insertPosition = result.Length - 3;

            while (insertPosition > 0)
            {
                result = result.Insert(insertPosition, ",");
                insertPosition -= 3;
            }

            return result;
        }

        public static float GetNormalizedFloatByLog(float input, float a, float aScaled, float b, float bScaled)
        {
            if (input <= 0 || a <= 0 || b <= 0)
            {
                Debug.LogWarning("Error: Do not pass negative values to Number.GetNormalizedFloatByLog()!!");
                return aScaled;
            }
            if (a > b) return GetNormalizedFloatByLog(input, b, bScaled, a, aScaled);
            
            float logA = Mathf.Log(a);
            float logB = Mathf.Log(b);
            float logInput = Mathf.Log(input);

            float t = Mathf.Clamp01((logInput - logA) / (logB - logA));
            return Mathf.Lerp(aScaled, bScaled, t);
        }
    }
}