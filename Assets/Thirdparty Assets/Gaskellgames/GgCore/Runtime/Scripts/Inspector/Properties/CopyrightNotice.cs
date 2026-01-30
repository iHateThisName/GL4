using System;
using UnityEngine;

namespace Gaskellgames
{
    /// <remarks>
    /// Code created by Gaskellgames: https://gaskellgames.com
    /// </remarks>
    
    [System.Serializable]
    public class CopyrightNotice : IComparable<CopyrightNotice>
    {
        #region Variables

        [Tooltip("The year of first publication.")]
        public int firstPublished;
        
        [Tooltip("The name of the person or entity that owns the copyright.")]
        public string ownerName;

        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region IComparable

        public int CompareTo(CopyrightNotice otherCopyrightNotice)
        {
            if (this < otherCopyrightNotice)
            {
                return -1;
            }
            
            if (this > otherCopyrightNotice)
            {
                return 1;
            }
            
            return 0;
        }
        
        public override bool Equals (object obj)
        {
            CopyrightNotice other = obj as CopyrightNotice;
            if (other == null) return false;
            return this.firstPublished == other.firstPublished && this.ownerName == other.ownerName;
        }
 
        public override int GetHashCode()
        {
            return GetNoticeLong().GetHashCode();
        }
        
        // define the is equal to operator
        public static bool operator == (CopyrightNotice noticeA, CopyrightNotice noticeB)
        {
            if (noticeA != null && noticeB != null)
            {
                return noticeA.CompareTo(noticeB) == 0;
            }
            else if (noticeA == null && noticeB != null)
            {
                return false;
            }
            else if (noticeA != null && noticeB == null)
            {
                return false;
            }
            else 
            {
                return true;
            }
        }
        
        // define the is not equal to operator
        public static bool operator != (CopyrightNotice noticeA, CopyrightNotice noticeB)
        {
            if (noticeA != null && noticeB != null)
            {
                return noticeA.CompareTo(noticeB) < 0 || 0 < noticeA.CompareTo(noticeB);
            }
            else if (noticeA == null && noticeB != null)
            {
                return true;
            }
            else if (noticeA != null && noticeB == null)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        
        // define the is less than operator
        public static bool operator < (CopyrightNotice noticeA, CopyrightNotice noticeB)
        {
            if (noticeA != null && noticeB != null)
            {
                // firstPublished
                if (noticeA.firstPublished < noticeB.firstPublished) { return true; }
            }
            
            return false;
        }
        
        // define the is more than operator
        public static bool operator > (CopyrightNotice noticeA, CopyrightNotice noticeB)
        {
            if (noticeA != null && noticeB != null)
            {
                // firstPublished
                if (noticeA.firstPublished > noticeB.firstPublished) { return true; }
            }
            
            return false;
        }

        #endregion
        
        //----------------------------------------------------------------------------------------------------

        #region Constructors

        public CopyrightNotice()
        {
            this.firstPublished = DateTime.Now.Year;
            this.ownerName = "Gaskellgames";
        }

        public CopyrightNotice(int firstPublished, int lastUpdated, string ownerName)
        {
            this.firstPublished = firstPublished;
            this.ownerName = ownerName;
        }

        #endregion
        
        //----------------------------------------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Get the copyright notice as a string in the format 'v{0}.{1}.{2}'
        /// </summary>
        /// <returns></returns>
        public string GetNoticeShort()
        {
            return $"\u00a9 {ownerName}.";
        }
        
        /// <summary>
        /// Get the semantic version as a string in the format 'Version {0}.{1}.{2}'
        /// </summary>
        /// <returns></returns>
        public string GetNoticeLong()
        {
            return $"Copyright \u00a9 {firstPublished} {ownerName}. All rights reserved.";
        }

        #endregion

    } // class end
}
