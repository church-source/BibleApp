using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MXit.Messaging;
using MXit.Messaging.MessageElements;
using MXit.Messaging.MessageElements.Actions;
using MXit.Messaging.MessageElements.Replies;
using MXit.User;
using MXit;
using MXit.Log;


using MySql.Data;
using MySql.Data.MySqlClient;



namespace MxitTestApp
{
    class UserFeedbackHandler : AInputHandler
    {
        public override InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved)
        {
            string input = extractReply(message_recieved);
            Console.WriteLine("User with ID: " + user_session.user_profile.id + " Entered: " + input);            
            //get reply
            string curr_user_page = user_session.current_menu_loc;

            InputHandlerResult output = handleStdNavLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            /*output = handleStdPageLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;*/

            MenuManager mm = MenuManager.getInstance();
            //for now we assume this. must correct this later
            VerseMenuPage vmp = (VerseMenuPage)mm.menu_def.getMenuPage(curr_user_page);
            List<MenuOptionItem> options = vmp.options;
            foreach (MenuOptionItem option in options)
            {
                if (option.link_val.Equals(input))
                    return new InputHandlerResult(
                         InputHandlerResult.NEW_MENU_ACTION,
                         option.select_action,
                         InputHandlerResult.DEFAULT_PAGE_ID);
            }

            if (input.Count() > MAX_MESSAGE_LENGTH)
            {
                return new InputHandlerResult(
                   "Your feedback message is too long, please keep it less than " + MAX_MESSAGE_LENGTH + " characters.\r\n"); //invalid choice
            }
            else if (input.Trim().Equals(""))
            {
                return new InputHandlerResult(
                   "You entered a blank message. please try again.\r\n"); //blank input
            }
            else
            {
                try
                {
                    saveUserFeedback(user_session, input);
                    sendUserFeedBackAsPrivateMessage(input, user_session);
                    return new InputHandlerResult(
                     InputHandlerResult.NEW_MENU_ACTION,
                     vmp.input_item.target_page,
                     InputHandlerResult.DEFAULT_PAGE_ID);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return new InputHandlerResult(
                   "Something went wrong when trying to store your feedback message, please try again later. "); //invalid choice
                }
            }

        }

        public void sendUserFeedBackAsPrivateMessage(String input, UserSession us)
        {

            String message_text = input;
            //String recip_id_s = "";
            long recip_id = UserNameManager.getInstance().getUserID(UserProfile.BIBLE_APP_USER_NAME);
            String start_verse = VerseMessageThread.NOTIFICATION_THREAD;
            String end_verse = VerseMessageThread.NOTIFICATION_THREAD;
            String subject = "User Feedback";

            us.verse_messaging_manager.createThreadAndAddPrivateMessage(
                message_text,
                recip_id,
                start_verse,
                end_verse,
                subject);
        }

        //check this for sql injection attack, I think parametrized query is enough protection
        public void saveUserFeedback(UserSession user_session, String input) 
        {
            MySqlConnection conn = DBManager.getConnection();
            try
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("INSERT INTO userfeedback "+
                    " VALUES(NULL,'" + 
                    user_session.user_profile.id + "','" + 
                    user_session.session_id + "','" +
                    DateTime.Now + "'," + 
                    "@user_feedback,'"+
                    "0');" , conn);

                cmd.Parameters.Add("@user_feedback", MySql.Data.MySqlClient.MySqlDbType.Text);

                cmd.Parameters["@user_feedback"].Value = input;
                cmd.ExecuteNonQuery();

            }
            finally
            {
                conn.Close();
            }
        }

        public const int MAX_MESSAGE_LENGTH = 1000;
        
    }


}
