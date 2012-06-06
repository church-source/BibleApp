/*
Software Copyright Notice

Copyright © 2004-2010 MXit Lifestyle Development Company (Pty) Ltd.
All rights are reserved

Copyright exists in this computer program and it is protected by
copyright law and by international treaties. The unauthorised use,
reproduction or distribution of this computer program constitute
acts of copyright infringement and may result in civil and criminal
penalties. Any infringement will be prosecuted to the maximum extent
possible.

MXit Lifestyle Development Company (Pty) Ltd chooses the following
address for delivery of all legal proceedings and notices:
  Riesling House,
  Brandwacht Office Park,
  Trumali Road,
  Stellenbosch,
  7600,
  South Africa.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sphinx.Client.Commands.Search;
using Sphinx.Client.Connections;
using System.Configuration;
using System.Reflection;

using MXit.Log;
//using Sphinx.Client.Commands.Search;
using MxitTestApp;
namespace MXit.ExternalApp.Examples.Redirect
{
    class ConsoleApplication
    {
        public ConsoleApplication()
        {
           
        }

        static void Main(string[] args)
        {


            ConsoleApplication ca = new ConsoleApplication();
            Console.Write("Initializing Random Code Engine...");
            String  randomCode = BibleUserCodeCreator.getInstance().generateUniqueANRandomCode(6);
            Console.Write(randomCode + "...");
            BibleUserCodeCreator.getInstance().generateANRandomCodesForExistingUsers(6);
            Console.WriteLine("Done");
            ExternalAppService<UserSession, BibleAppEngine> externalApp = null;
            try
            {
                try
                {
                    IList<SearchQueryResult> results = BibleSearch.getInstance().searchBible("For & God & loved", 2, 42, -1);

                    foreach (SearchQueryResult result in results)
                    {
                        foreach (Match match in result.Matches)
                        {
                            Console.WriteLine("Document ID: {0}", match.DocumentId);
                            Console.WriteLine("Weight: {0}", match.Weight);
                            Console.WriteLine("book: " + BibleHelper.getNameFromID(match.AttributesValues["book"].GetValue().ToString()));
                            Console.WriteLine("chapter: " + match.AttributesValues["chapter"].GetValue());
                            Console.WriteLine("verse: " + match.AttributesValues["verse"].GetValue());
                            // Console.WriteLine(match.AttributesValues["versetext"]);
                        }
                        Console.WriteLine("Elapsed time (ms): " + result.ElapsedTime.TotalMilliseconds);
                        Console.WriteLine("Total found: " + result.TotalFound);
                        Console.WriteLine("Returned matches count: " + result.Count);
                    }
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.Message);
                    Console.WriteLine(e1.StackTrace);
                }


    /*            try
                {
                    
                    foreach (DailyVerse dv in list)
                    {
                        Console.WriteLine(dv);
                    }
                }
                catch (Exception e)
                {

                }*/

                Console.WriteLine("Loading Topics");
                BibleTopicManager.getInstance();
                Console.WriteLine("Done");
                SuspensionManager.getInstance();
                Console.Write("Initializing Colour Themes...");
                UserColourTheme.getColourTheme(UserColourTheme.NO_THEME);
                Console.WriteLine("Done");
                /*initialise Bibles*/
                Console.WriteLine("Loading Bible Text into Memory...");
                Console.WriteLine(BibleContainer.getInstance().getBible(0).testaments[1].getBook("John").getChapter(3).getVerse(16).text);
                Console.WriteLine(BibleContainer.getInstance().getBible(1).testaments[1].getBook("John").getChapter(3).getVerse(16).text);
                Console.WriteLine(BibleContainer.getInstance().getBible(2).testaments[1].getBook("John").getChapter(3).getVerse(16).text);
                Console.WriteLine("Load Completed");

                Console.WriteLine("Initializing Daily Verse function...");
                //this has to be done after the Bible Load
                DailyVerseObservable.getInstance();
                Console.WriteLine("Done");

                
                // Read the ExternalApp's configuration
                string name = ConfigurationManager.AppSettings["ExternalApp.Name"].ToString();
                string password = ConfigurationManager.AppSettings["ExternalApp.Password"].ToString();
                int requestQueueThreads = int.Parse(ConfigurationManager.AppSettings["ExternalApp.RequestQueueThreads"].ToString());
                int requestQueueMaxCount = int.Parse(ConfigurationManager.AppSettings["ExternalApp.RequestQueueMaxCount"].ToString());
                string adminBotName = ConfigurationManager.AppSettings["ExternalAppAPI.AdminBot"].ToString();

                BibleAppEngine redirectLogicEngine = new BibleAppEngine(adminBotName);

                // Create a new ExternalApp service instance
                externalApp = new ExternalAppService<UserSession, BibleAppEngine>(
                    redirectLogicEngine,
                    name,
                    password,
                    requestQueueThreads,
                    requestQueueMaxCount
                    );

                // Start the ExternalApp
                externalApp.Start();
                //NETBibleLoader.loadNetBible("C:\\Users\\rpillay\\Documents\\Bibles\\NET\\netfreesplit\\netfreesplit\\");
                // Log some info
                ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "Press any key to stop.", Level.Info);

                // Wait till a key is pressed
                Console.ReadKey(true);
            }
            catch (Exception e)
            {
                // Log some info
                ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "An error occurred:\n" + e.ToString(), Level.Error);

                // Wait till a key is pressed
                Console.ReadKey(true);
            }
            finally
            {
                // If the ExternalApp is not stopped
                if (externalApp != null && externalApp.Status != ExternalAppServiceStatus.Stopped)
                {
                    // Log some info
                    ConsoleLogger.Log(MethodBase.GetCurrentMethod(), "About to stop ExternalApp", Level.Info);

                    // Stop the ExternalApp
                    externalApp.Stop();
                }
            }
        }

  
    }
}
