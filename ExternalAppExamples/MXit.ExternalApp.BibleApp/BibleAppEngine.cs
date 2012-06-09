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

using System.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using MXit.Messaging;
using MXit.Navigation;
using MXit.Messaging.MessageElements;
using MxitTestApp;
using MXit.ExternalApp;

using MySql.Data;
using MySql.Data.MySqlClient;

using System.Timers;

namespace MXit.ExternalApp.Examples.Redirect
{
    /// <summary>
    /// Contains the logic behind the redirect example.
    /// </summary>
    public class BibleAppEngine : ExternalAppLogicEngine<UserSession>, IDailyVerseObserver
    {
        /// <summary>
        /// The ExternalAppAPI admin bot's service name.
        /// </summary>
        public string ExternalAppApiAdminBotName { get; private set; }
        //private DailyVerse daily_verse = null;


        /// <summary>
        /// An event handler that will be invoked whenever a message is received from a MXit user.
        /// </summary>
        /// <param name="messageReceived">The message that was received from the MXit user.</param>
        /// <param name="userSession">The user's session object.</param>
        public override void OnMessageReceived(MessageReceived messageReceived, UserSession userSession)
        {
            DateTime start = DateTime.Now;
            try
            {
                userSession.initializeUserSession(
                    messageReceived,
                    ExternalAppApiComms.GetUser(messageReceived.From));
                MessageToSend messageToSend = userSession.handleInput(messageReceived);
                //we need to the change the above method to return a result class, that gives more info, then send the message which will be an 
                //member var of the result class
                if (messageToSend != null)
                {
                    ExternalAppApiComms.SendMessage(messageToSend);
                }
                else
                {
                    ExternalAppApiComms.RedirectUser(messageReceived.CreateRedirectRequest("communityportal", "RecommendPage?ItemId=4221728"));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                try
                {
                    MessageToSend messageToSend = userSession.handleError(messageReceived);
                    messageToSend.Clear();
                    messageToSend.Append(MessageBuilder.Elements.CreateClearScreen());
                    messageToSend.Append("Something went wrong. Please let us know what happened, by sending us feedback. You can access the feedback option from the Main Menu.\r\n");
                    messageToSend.Append(createMessageLink(MENU_LINK_NAME, "Main", "MAIN"));
                    ExternalAppApiComms.SendMessage(messageToSend);
                }
                catch (Exception e_2)
                {
                    Console.WriteLine(e_2.Message);
                    Console.WriteLine(e_2.StackTrace);
                }
            }
            DateTime end = DateTime.Now;
            TimeSpan ts = end.Subtract(start);
            Console.WriteLine("Time taken to process Message--------------------------------------------------");
            Console.WriteLine("Total Seconds + (mill in Milli): " + ((ts.Seconds * 1000) + ts.Milliseconds));
            Console.WriteLine("-------------------------------------------------------------------------------");
        }

        public IMessageElement createMessageLink(string name, string display, string reply)
        {
            IMessageElement link = MessageBuilder.Elements.CreateLink(name,          // Optional
                                                          display,             // Compulsory
                                                          null,  // Optional
                                                          reply);        // Optional 
            return link;
        }




        /// <summary>
        /// Initializes a new instance of the <see cref="BibleAppEngine"/> class.
        /// </summary>
        /// <param name="externalAppApiAdminBotName">The ExternalAppAPI admin bot's service name.</param>
        public BibleAppEngine(string externalAppApiAdminBotName)
        {
            ExternalAppApiAdminBotName = externalAppApiAdminBotName;
            //load daily verses.
            DailyVerseObservable.getInstance();
            //start the timer.
            restartTimer();

            //Thread.Sleep(10000);
            //sendDailyVerse(null, null);
        }

        public void restartTimer()
        {
            System.Timers.Timer myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(sendDailyVerse);

            //calcuate time until 3am next morning when we will start sending daily verses. 
            DateTime current_time = DateTime.Now;
            DateTime time_to_send = DateTime.Now;
            if (current_time.Hour >= 0 && current_time.Hour < 6)
            {
                //send on current day
                time_to_send = current_time.Date;
                time_to_send = time_to_send.AddHours(6);
            }
            else
            {
                time_to_send = current_time.Date.AddDays(1);
                time_to_send = time_to_send.AddHours(3);
            }
            double millis = (time_to_send.Subtract(current_time)).TotalMilliseconds;
            myTimer.Interval = millis;
            //GetDailyVerse(null,null);//call it just after loading
            myTimer.Start();
        }

        public void updateDailyVerse(DailyVerse daily_verse)
        {
            Console.WriteLine("________________________________________________________________________________________________________");
            Console.WriteLine("New Daily Verse has been loaded. Send this to users!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine(daily_verse);
            Console.WriteLine("________________________________________________________________________________________________________");
        }


        public void sendDailyVerse(object source, ElapsedEventArgs e)
        {
            int error_count = 0;
            int send_count = 0;

            if (source != null)
            {
                ((System.Timers.Timer)source).Stop();
                ((System.Timers.Timer)source).Dispose();
            }

            

            DailyVerse daily_verse = DailyVerseObservable.getInstance().loadLatestDailyVerse();
            if (daily_verse == null)
                return;

            Console.WriteLine("________________________________________________________________________________________________________");
            Console.WriteLine("Attempting to send daily Verse to subscribed users");
            Console.WriteLine(daily_verse);
            Console.WriteLine("________________________________________________________________________________________________________");

            string sqlQuery = "SELECT user_id, subscribed_to_daily_verse,id,theme  FROM userprofile where subscribed_to_daily_verse = 1";
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                String user_id = "";
                MessageToSend messageToSend;
                String user_name = "";
                DateTime nowdate = DateTime.Now;
                List<MessageToSend> messages = new List<MessageToSend>();
                List<DailyVerseRecipient> recip_list = new List<DailyVerseRecipient>();
                try
                {
                    while (rdr.Read())
                    {
                        try
                        {
                            recip_list.Add(
                                new DailyVerseRecipient(
                                    (rdr[0]).ToString(),                    //MXit user_id
                                    long.Parse((rdr[2]).ToString()),        //local id
                                    Int32.Parse((rdr[3]).ToString())));      //theme
                        
                        }
                        catch(Exception e1)
                        {
                            Console.WriteLine("Error: " + e1.Message + " \r\n " + e1.StackTrace);
                        }
                    }
                }
                catch(Exception e2)
                {
                    Console.WriteLine(e2.Message);
                    Console.WriteLine(e2.StackTrace);
                
                }
                Console.WriteLine("__________________________________________________________"); 
                Console.WriteLine("Sending DailyVese to: " + recip_list.Count + " users");
                Console.WriteLine("__________________________________________________________");
                foreach (DailyVerseRecipient dvr in recip_list)
                {
                    try
                    {
                        user_id = dvr.user_id;
                        //is_subscribed = Boolean.Parse((rdr[1]).ToString());
                        user_name = UserNameManager.getUserName(dvr.id);
                        messageToSend = new MessageToSend("", user_id, User.DeviceInfo.DefaultDevice);
                        messageToSend.MaySpool = true;
                        messageToSend.Append(MessageBuilder.Elements.CreateClearScreen());
                        UserColourTheme uct = UserColourTheme.getColourTheme(dvr.theme);

                        messageToSend.Body = "Hi " + user_name+
                        "\r\n\r\nYour Daily Verse For " + nowdate.ToString("dd/MM/yyyy") + "... \r\n\r\n" +
                        daily_verse.verse_ref+
                        "\r\n" + daily_verse.verse_text + "\r\n\r\n";
                        messageToSend.Append(createMessageLink(MENU_LINK_NAME, "Click here to continue to the BibleApp", "."));
                        if(uct!=null)
                            messageToSend.AppendLine("\r\n\r\nTip: You can opt out of receiving a daily verse on your profile option (option 5) in the BibleApp", uct.getTipTextColour());
                        else
                            messageToSend.AppendLine("\r\n\r\nTip: You can opt out of receiving a daily verse on your profile option (option 5) in the BibleApp");
                        IMessageElement chatScreenConfig;
                        IClientColors clientColors = MessageBuilder.Elements.CreateClientColors(); //Create the colour scheme you want to 
                        if (uct != null)
                        {
                            clientColors[ClientColorType.Background] = uct.getBackGroundColour();
                            clientColors[ClientColorType.Text] = uct.getForeGroundColour();
                            clientColors[ClientColorType.Link] = uct.getLinkColour();
                            chatScreenConfig = MessageBuilder.Elements.CreateChatScreenConfig(
                                ChatScreenBehaviourType.ShowProgress |
                                ChatScreenBehaviourType.NoPrefix,
                                clientColors);
                            messageToSend.Append(chatScreenConfig);
                        }

                        //ExternalAppApiComms.SendMessage(messageToSend);
                        messages.Add(messageToSend);
                        send_count++;
                        //after every 30 messages we wait a bit to let messags in queue to get sent
                        if (send_count % 30 == 0)
                        {
                            BatchJobData bjd = new BatchJobData();
                            bjd.engine = this;
                            bjd.messages = messages;
                            Thread thread = new Thread(sendBatchMessages);
                            Console.WriteLine("Spawning daily verse thread to send to batch of users, max user id = " + user_id);
                            thread.Start(bjd);
                            Thread.Sleep(5000);
                            messages = new List<MessageToSend>();
                        }
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1.Message);
                        Console.WriteLine(e1.StackTrace);
                        error_count++;
                    }
                }
                //are there still some verses left? 
                if (messages != null && messages.Count > 0)
                {
                    try
                    {
                        Console.WriteLine("Spawning daily verse thread to send to final batch of users");
                        BatchJobData bjd = new BatchJobData();
                        bjd.engine = this;
                        bjd.messages = messages;
                        Thread thread = new Thread(sendBatchMessages);
                        thread.Start(bjd);
                        Thread.Sleep(5000);
                        messages = new List<MessageToSend>();
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1.Message);
                        Console.WriteLine(e1.StackTrace);
                        error_count++;
                    }
                }
                Console.WriteLine("________________________________________________________________________________________________________");
                Console.WriteLine("Done Sending Daily Verses to " + send_count + " subscribers.");
                Console.WriteLine("________________________________________________________________________________________________________");
            }
            finally
            {
                conn.Close();
                if (daily_verse != null)
                {
                    DateTime now = DateTime.Now;
                    daily_verse.is_sent = true;
                    daily_verse.sent_datetime = now;
                    DailyVerseObservable.getInstance().updateSentStatusOfDailyVerseToSent(daily_verse.id,now);
                }
                restartTimer();
            }
        }

        public static void sendBatchMessages(Object obj)
        {
            BatchJobData bjd = (BatchJobData)obj;
            foreach (MessageToSend message in bjd.messages)
            {
                try
                {
                    bjd.engine.ExternalAppApiComms.SendMessage(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            Console.WriteLine("Finished sending batch of daily messages");
        }

        public const string MENU_LINK_NAME = "menu_link";

        public class DailyVerseRecipient
        {
            public String user_id {get;set;}
            public long id {get;set;}
            public int theme {get;set;}

            public DailyVerseRecipient(String user_id, long id, int theme)
            {
                this.user_id = user_id;
                this.id = id;
                this.theme = theme;
            }
        }
    }
}
