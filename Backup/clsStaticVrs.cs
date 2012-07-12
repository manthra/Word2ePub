using System;
using System.Collections.Generic;
using System.Text;

namespace ORC
{
    class clsStaticVrs
    {
        public static string strID = "OEBPS";
        public static string[,] strPUKFileNamesTemp;
        public static string strclsFolderName = "";
        public static string strclsTitle = "";

        public static string getBookTitle()
        {
            return strclsTitle;
        }

        public static void setBookTitle(string strBkTitle)
        {
            strclsTitle = strBkTitle;
        }



        public static string getFolderNameSaved()
        {
            return strclsFolderName;
        }

        public static void  setFolderNameSaved(string strFldNm)
        {
            strclsFolderName = strFldNm;
        }


        public static string getID()
        {
            return strID; 
        }

        public static void setID(string strNewID)
        {
            strID = strNewID;
        }

        public static void setFileNames(string[,] strNewfileName)
        {
            strPUKFileNamesTemp = strNewfileName;
        }

        public static string[,] getFileNames()
        {
            return strPUKFileNamesTemp;
        }


    }
}
