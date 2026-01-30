using System;
using UnityEngine;

namespace Gaskellgames
{
    /// <remarks>
    /// Code created by Gaskellgames: https://gaskellgames.com
    /// </remarks>
    
    public static class LerpExtensions
    {
        #region InverseLerp
        
        /// <summary>
        /// Determines where a value lies between two points.
        /// </summary>
        /// <param name="a">The start of the range.</param>
        /// <param name="b">The end of the range.</param>
        /// <param name="value">The point within the range you want to calculate.</param>
        /// <returns>A value between zero and one, representing where the "value" parameter falls within the range defined by a and b.</returns>
        public static float InverseLerp(float a, float b, float value)
        {
            return (double) a != (double) b
                ? Mathf.Clamp01((float) (((double) value - (double) a) / ((double) b - (double) a)))
                : 0.0f;
        }
        
        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region Lerp
        
        /// <summary>
        /// Linearly interpolates between a and b by t.
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value.</param>
        /// <param name="t">The interpolation value between the two floats.</param>
        /// <returns>The interpolated float result between the two float values.</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Mathf.Clamp01(t);
        }
        
        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region LerpUnclamped
        
        /// <summary>
        /// Linearly interpolates between a and b by t with no limit to t.
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value.</param>
        /// <param name="t">The interpolation between the two floats.</param>
        /// <returns>The float value as a result from the linear interpolation.</returns>
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        
        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region LerpAngle
        
        /// <summary>
        /// Same as Lerp but makes sure the values interpolate correctly when they wrap around 360 degrees.
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value.</param>
        /// <param name="t">The interpolation value between the two floats.</param>
        /// <returns>The interpolated float result between the two float values.</returns>
        public static float LerpAngle(float a, float b, float t)
        {
            float num = Mathf.Repeat(b - a, 360f);
            if ((double)num > 180.0) { num -= 360f; }
            return a + num * Mathf.Clamp01(t);
        }
        
        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region LerpEasing
        
        /// <summary>
        /// Same as Lerp, but using a specified easing function (default is linear interpolation).
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value.</param>
        /// <param name="t">The interpolation value between the two floats.</param>
        /// <param name="easingFunction">The easing function to use in place of linear interpolation.</param>
        /// <returns>The interpolated float result between the two float values.</returns>
        public static float LerpEasing(float a, float b, float t, EasingFunction easingFunction = EasingFunction.Linear)
        {
            float linear = Lerp(a, b, t);
            
            switch (easingFunction)
            {
                default:
                case EasingFunction.Linear:
                    return linear;
                
                case EasingFunction.InQuadratic:
                    return InQuadratic(linear);
                case EasingFunction.OutQuadratic:
                    return OutQuadratic(linear);
                case EasingFunction.InOutQuadratic:
                    return InOutQuadratic(linear);
                
                case EasingFunction.InCubic:
                    return InCubic(linear);
                case EasingFunction.OutCubic:
                    return OutCubic(linear);
                case EasingFunction.InOutCubic:
                    return InOutCubic(linear);
                
                case EasingFunction.InQuartic:
                    return InQuartic(linear);
                case EasingFunction.OutQuartic:
                    return OutQuartic(linear);
                case EasingFunction.InOutQuartic:
                    return InOutQuartic(linear);
                
                case EasingFunction.InQuintic:
                    return InQuintic(linear);
                case EasingFunction.OutQuintic:
                    return OutQuintic(linear);
                case EasingFunction.InOutQuintic:
                    return InOutQuintic(linear);
                
                case EasingFunction.InSine:
                    return InSine(linear);
                case EasingFunction.OutSine:
                    return OutSine(linear);
                case EasingFunction.InOutSine:
                    return InOutSine(linear);
                
                case EasingFunction.InExponential:
                    return InExponential(linear);
                case EasingFunction.OutExponential:
                    return OutExponential(linear);
                case EasingFunction.InOutExponential:
                    return InOutExponential(linear);
                
                case EasingFunction.InCircular:
                    return InCircular(linear);
                case EasingFunction.OutCircular:
                    return OutCircular(linear);
                case EasingFunction.InOutCircular:
                    return InOutCircular(linear);
                
                case EasingFunction.InElastic:
                    return InElastic(linear);
                case EasingFunction.OutElastic:
                    return OutElastic(linear);
                case EasingFunction.InOutElastic:
                    return InOutElastic(linear);
                
                case EasingFunction.InBack:
                    return InBack(linear);
                case EasingFunction.OutBack:
                    return OutBack(linear);
                case EasingFunction.InOutBack:
                    return InOutBack(linear);
                
                case EasingFunction.InBounce:
                    return InBounce(linear);
                case EasingFunction.OutBounce:
                    return OutBounce(linear);
                case EasingFunction.InOutBounce:
                    return InOutBounce(linear);
            }
        }
        
        #endregion
        
    	//----------------------------------------------------------------------------------------------------
        
        #region EasingFunctions
        
        private static float InQuadratic(float t) => t * t;
        private static float OutQuadratic(float t) => 1 - InQuadratic(1 - t);
        private static float InOutQuadratic(float t) => (t < 0.5) ? (InQuadratic(t * 2) * 0.5f) : (1 - (InQuadratic((1 - t) * 2) * 0.5f));

        private static float InCubic(float t) => t * t * t;
		private static float OutCubic(float t) => 1 - InCubic(1 - t);
		private static float InOutCubic(float t) => (t < 0.5) ? (InCubic(t * 2) * 0.5f) : (1 - (InCubic((1 - t) * 2) * 0.5f));

		private static float InQuartic(float t) => t * t * t * t;
		private static float OutQuartic(float t) => 1 - InQuartic(1 - t);
		private static float InOutQuartic(float t) => (t < 0.5) ? (InQuartic(t * 2) * 0.5f) : (1 - (InQuartic((1 - t) * 2) * 0.5f));

		private static float InQuintic(float t) => t * t * t * t * t;
		private static float OutQuintic(float t) => 1 - InQuintic(1 - t);
		private static float InOutQuintic(float t) => (t < 0.5) ? (InQuintic(t * 2) * 0.5f) : (1 - (InQuintic((1 - t) * 2) * 0.5f));

		private static float InSine(float t) => 1 - (float)Math.Cos(t * Math.PI * 0.5f);
		private static float OutSine(float t) => (float)Math.Sin(t * Math.PI * 0.5f);
		private static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) * -0.5f;

		private static float InExponential(float t) => (float)Math.Pow(2, 10 * (t - 1));
		private static float OutExponential(float t) => 1 - InExponential(1 - t);
		private static float InOutExponential(float t) => (t < 0.5) ? (InExponential(t * 2) * 0.5f) : (1 - (InExponential((1 - t) * 2) * 0.5f));

		private static float InCircular(float t) => -((float)Math.Sqrt(1 - t * t) - 1);
		private static float OutCircular(float t) => 1 - InCircular(1 - t);
		private static float InOutCircular(float t) => (t < 0.5) ? (InCircular(t * 2) * 0.5f) : (1 - (InCircular((1 - t) * 2) * 0.5f));

		private static float InElastic(float t) => 1 - OutElastic(1 - t);
		private static float OutElastic(float t, float p = 0.3f) => (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - p * 0.25f) * (2 * Math.PI) / p) + 1;
		private static float InOutElastic(float t) => (t < 0.5) ? (InElastic(t * 2) * 0.5f) : (1 - (InElastic((1 - t) * 2) * 0.5f));

		private static float InBack(float t, float s = 1.70158f) => t * t * ((s + 1) * t - s);
		private static float OutBack(float t) => 1 - InBack(1 - t);
		private static float InOutBack(float t) => (t < 0.5) ? (InBack(t * 2) * 0.5f) : (1 - (InBack((1 - t) * 2) * 0.5f));

		private static float InBounce(float t) => 1 - OutBounce(1 - t);
		private static float OutBounce(float t, float div = 2.75f, float mult = 7.5625f)
		{
			if (t < 1 / div)
			{
				return mult * t * t;
			}
            else if (t < 2 / div)
			{
				t -= 1.5f / div;
				return mult * t * t + 0.75f;
			}
            else if (t < 2.5 / div)
			{
				t -= 2.25f / div;
				return mult * t * t + 0.9375f;
			}
            else
            {
                t -= 2.625f / div;
                return mult * t * t + 0.984375f;
            }
		}
		private static float InOutBounce(float t) => (t < 0.5) ? (InBounce(t * 2) * 0.5f) : (1 - (InBounce((1 - t) * 2) * 0.5f));

        #endregion

    } // class end
}